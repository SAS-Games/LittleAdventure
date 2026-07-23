using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LevelStreaming
{
    /// <summary>
    /// Shares an asynchronously loaded resource between regions that use the same key.
    /// Entries are transactional: failed loads are removed and the final release owns
    /// exactly one unload operation.
    /// </summary>
    public sealed class SharedStreamingRegistry
    {
        private sealed class Entry
        {
            public int RefCount;
            public Type ValueType;
            public Task<object> LoadingTask;
            public Func<Task> Unload;
            public Task UnloadingTask;
        }

        public readonly struct EntrySnapshot
        {
            public EntrySnapshot(string key, int referenceCount, bool isLoading, bool isUnloading, bool loadFailed)
            {
                Key = key;
                ReferenceCount = referenceCount;
                IsLoading = isLoading;
                IsUnloading = isUnloading;
                LoadFailed = loadFailed;
            }

            public string Key { get; }
            public int ReferenceCount { get; }
            public bool IsLoading { get; }
            public bool IsUnloading { get; }
            public bool LoadFailed { get; }
        }

        private sealed class StateOnlyResource
        {
        }

        private readonly object _gate = new();
        private readonly Dictionary<string, Entry> _entries = new(StringComparer.Ordinal);

        public int Count
        {
            get
            {
                lock (_gate)
                    return _entries.Count;
            }
        }

        public async Task Acquire(string key, Func<Task> load, Func<Task> unload)
        {
            if (load == null)
                throw new ArgumentNullException(nameof(load));

            await AcquireCore(
                key,
                typeof(StateOnlyResource),
                async () =>
                {
                    await load();
                    return null;
                },
                unload);
        }

        public async Task<T> Acquire<T>(string key, Func<Task<T>> load, Func<Task> unload)
        {
            if (load == null)
                throw new ArgumentNullException(nameof(load));

            object value = await AcquireCore(
                key,
                typeof(T),
                async () => await load(),
                unload);

            return (T)value;
        }

        public async Task Release(string key)
        {
            ValidateKey(key);

            Task unloadingTask;
            Entry entryToUnload = null;
            TaskCompletionSource<bool> unloadCompletion = null;
            lock (_gate)
            {
                if (!_entries.TryGetValue(key, out var entry))
                    throw new InvalidOperationException(
                        $"Streaming key '{key}' has no acquired registry entry.");

                if (entry.UnloadingTask != null)
                {
                    // A repeated final release observes the same operation rather than
                    // decrementing below zero or starting a second unload.
                    unloadingTask = entry.UnloadingTask;
                }
                else
                {
                    if (entry.RefCount > 0)
                        entry.RefCount--;

                    if (entry.RefCount > 0)
                        return;

                    // Reaching zero transfers final ownership to exactly one unload.
                    unloadingTask = BeginUnloadNoLock(entry, out unloadCompletion);
                    entryToUnload = entry;
                }
            }

            if (entryToUnload != null)
                RunUnload(key, entryToUnload, unloadCompletion);

            await unloadingTask;
        }

        public bool TryGetSnapshot(string key, out EntrySnapshot snapshot)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                snapshot = default;
                return false;
            }

            lock (_gate)
            {
                if (!_entries.TryGetValue(key, out var entry))
                {
                    snapshot = default;
                    return false;
                }

                snapshot = new EntrySnapshot(
                    key,
                    entry.RefCount,
                    !entry.LoadingTask.IsCompleted,
                    entry.UnloadingTask is { IsCompleted: false },
                    entry.LoadingTask.IsFaulted);
                return true;
            }
        }

        public IReadOnlyList<EntrySnapshot> GetSnapshots()
        {
            lock (_gate)
            {
                var snapshots = new List<EntrySnapshot>(_entries.Count);
                foreach (var pair in _entries)
                {
                    var entry = pair.Value;
                    snapshots.Add(new EntrySnapshot(
                        pair.Key,
                        entry.RefCount,
                        !entry.LoadingTask.IsCompleted,
                        entry.UnloadingTask is { IsCompleted: false },
                        entry.LoadingTask.IsFaulted));
                }

                return snapshots;
            }
        }

        private async Task<object> AcquireCore(string key, Type valueType, Func<Task<object>> load,
            Func<Task> unload)
        {
            ValidateKey(key);
            if (valueType == null)
                throw new ArgumentNullException(nameof(valueType));
            if (unload == null)
                throw new ArgumentNullException(nameof(unload));

            while (true)
            {
                Task waitForUnload = null;
                Task<object> loadingTask = null;
                Entry entryToLoad = null;
                TaskCompletionSource<object> loadCompletion = null;

                lock (_gate)
                {
                    if (_entries.TryGetValue(key, out var existing))
                    {
                        if (existing.UnloadingTask != null)
                        {
                            waitForUnload = existing.UnloadingTask;
                        }
                        else
                        {
                            if (existing.ValueType != valueType)
                            {
                                throw new InvalidOperationException(
                                    $"Streaming key '{key}' is already registered as " +
                                    $"{existing.ValueType.Name}, not {valueType.Name}.");
                            }

                            checked
                            {
                                existing.RefCount++;
                            }

                            loadingTask = existing.LoadingTask;
                        }
                    }
                    else
                    {
                        loadCompletion = new TaskCompletionSource<object>(
                            TaskCreationOptions.RunContinuationsAsynchronously);
                        var entry = new Entry
                        {
                            RefCount = 1,
                            ValueType = valueType,
                            LoadingTask = loadCompletion.Task,
                            Unload = unload
                        };

                        _entries.Add(key, entry);
                        entryToLoad = entry;
                        loadingTask = entry.LoadingTask;
                    }
                }

                // Backend code may complete synchronously or re-enter the registry, so
                // it must never be invoked while the registry lock is held.
                if (entryToLoad != null)
                    RunLoad(key, entryToLoad, load, loadCompletion);

                if (waitForUnload != null)
                {
                    try
                    {
                        await waitForUnload;
                    }
                    catch
                    {
                        // A failed unload leaves the successfully loaded entry available.
                        // Looping lets this acquire take ownership of that entry instead of
                        // starting a duplicate load whose predecessor may still be resident.
                    }

                    continue;
                }

                return await loadingTask;
            }
        }

        private void RunLoad(string key, Entry entry, Func<Task<object>> load,
            TaskCompletionSource<object> completion)
        {
            _ = RunLoadAsync();

            async Task RunLoadAsync()
            {
                try
                {
                    completion.TrySetResult(await load());
                }
                catch (Exception exception)
                {
                    lock (_gate)
                    {
                        if (_entries.TryGetValue(key, out var current) && ReferenceEquals(current, entry))
                            _entries.Remove(key);
                    }

                    completion.TrySetException(exception);
                }
            }
        }

        private static Task BeginUnloadNoLock(Entry entry,
            out TaskCompletionSource<bool> completion)
        {
            completion = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            entry.UnloadingTask = completion.Task;
            return completion.Task;
        }

        private void RunUnload(string key, Entry entry, TaskCompletionSource<bool> completion)
        {
            _ = RunUnloadAsync();

            async Task RunUnloadAsync()
            {
                try
                {
                    // Shutdown can release an entry before its load has completed.
                    await entry.LoadingTask;
                    await entry.Unload();

                    lock (_gate)
                    {
                        if (_entries.TryGetValue(key, out var current) && ReferenceEquals(current, entry))
                            _entries.Remove(key);
                    }

                    completion.TrySetResult(true);
                }
                catch (Exception exception)
                {
                    lock (_gate)
                    {
                        if (_entries.TryGetValue(key, out var current) && ReferenceEquals(current, entry))
                        {
                            // Physical unload failed, so the resource is still considered
                            // owned by the releasing caller and can be retried safely.
                            entry.RefCount = Math.Max(1, entry.RefCount);
                            entry.UnloadingTask = null;
                        }
                    }

                    completion.TrySetException(exception);
                }
            }
        }

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("A non-empty streaming key is required.", nameof(key));
        }
    }
}
