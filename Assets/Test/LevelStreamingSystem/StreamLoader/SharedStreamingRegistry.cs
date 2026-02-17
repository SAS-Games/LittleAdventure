using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LevelStreaming
{
    public class SharedStreamingRegistry
    {
        private class Entry
        {
            public int RefCount;
            public Task<object> LoadingTask;
            public Func<Task> Unload;
        }

        private readonly Dictionary<string, Entry> _entries = new();

        // ---------- STATE ONLY (Scenes) ----------
        public async Task Acquire(
            string key,
            Func<Task> load,
            Func<Task> unload)
        {
            if (_entries.TryGetValue(key, out var existing))
            {
                existing.RefCount++;
                await existing.LoadingTask;
                return;
            }

            var entry = new Entry
            {
                RefCount = 1,
                Unload = unload,
                LoadingTask = Wrap(load)
            };

            _entries.Add(key, entry);
            await entry.LoadingTask;
        }

        // ---------- ASSET RETURN (Prefabs) ----------
        public async Task<T> Acquire<T>(
            string key,
            Func<Task<T>> load,
            Func<Task> unload)
        {
            if (_entries.TryGetValue(key, out var existing))
            {
                existing.RefCount++;
                return (T)await existing.LoadingTask;
            }

            var entry = new Entry
            {
                RefCount = 1,
                Unload = unload,
                LoadingTask = Wrap(load)
            };

            _entries.Add(key, entry);
            return (T)await entry.LoadingTask;
        }

        public async Task Release(string key)
        {
            if (!_entries.TryGetValue(key, out var entry))
                return;

            entry.RefCount--;

            if (entry.RefCount > 0)
                return;

            await entry.Unload();
            _entries.Remove(key);
        }

        private async Task<object> Wrap(Func<Task> load)
        {
            await load();
            return null;
        }

        private async Task<object> Wrap<T>(Func<Task<T>> load)
        {
            return await load();
        }
    }
}