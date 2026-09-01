using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace SAS.Checkpoints
{
    /// <summary>
    /// Portable default store. It intentionally uses only Unity and the BCL so
    /// the checkpoint folder does not require a project-specific save package.
    /// </summary>
    public sealed class JsonFileCheckpointProgressStore : ICheckpointProgressStore
    {
        private const string DirectoryName = "Progress";
        private const string FileName = "CheckpointProgress.json";

        private readonly string _rootDirectory;

        public JsonFileCheckpointProgressStore(string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
                throw new ArgumentException("A save root directory is required.", nameof(rootDirectory));

            _rootDirectory = rootDirectory;
        }

        public async Task<CheckpointProgressData> LoadAsync(int userId)
        {
            string path = GetPath(userId);

            if (!File.Exists(path))
                return new CheckpointProgressData();

            string json = await File.ReadAllTextAsync(path);

            if (string.IsNullOrWhiteSpace(json))
                return new CheckpointProgressData();

            return JsonUtility.FromJson<CheckpointProgressData>(json) ??
                   new CheckpointProgressData();
        }

        public async Task<bool> SaveAsync(int userId, CheckpointProgressData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            string path = GetPath(userId);
            string directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string temporaryPath = path + ".tmp";
            string json = JsonUtility.ToJson(data, true);

            await File.WriteAllTextAsync(temporaryPath, json);

            try
            {
                if (File.Exists(path))
                    File.Replace(temporaryPath, path, null);
                else
                    File.Move(temporaryPath, path);
            }
            catch
            {
                if (File.Exists(path))
                    File.Delete(path);

                File.Move(temporaryPath, path);
            }

            return true;
        }

        private string GetPath(int userId)
        {
            return Path.Combine(
                _rootDirectory,
                userId.ToString(),
                DirectoryName,
                FileName);
        }
    }
}
