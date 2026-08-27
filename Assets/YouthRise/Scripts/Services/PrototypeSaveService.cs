using System;
using System.IO;
using UnityEngine;

namespace YouthRise
{
    [Serializable]
    public sealed class PrototypeSave
    {
        public string chapterId;
        public string currentNodeId;
        public string branchPath;
        public bool chapterCompleted;
        public string updatedUtc;
        public PlayerProfile profile;
    }

    public static class PrototypeSaveService
    {
        private const string SaveFileName = "prototype-save.json";

        public static bool Exists => File.Exists(GetPath());

        public static void Save(PrototypeSave save)
        {
            if (save == null)
                return;

            try
            {
                string path = GetPath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                save.updatedUtc = DateTime.UtcNow.ToString("O");

                string temporaryPath = path + ".tmp";
                File.WriteAllText(temporaryPath, JsonUtility.ToJson(save, true));
                if (File.Exists(path))
                    File.Delete(path);
                File.Move(temporaryPath, path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"YouthRise could not save progress: {exception.Message}");
            }
        }

        public static bool TryLoad(out PrototypeSave save)
        {
            save = null;
            try
            {
                string path = GetPath();
                if (!File.Exists(path))
                    return false;

                save = JsonUtility.FromJson<PrototypeSave>(File.ReadAllText(path));
                return save != null && save.profile != null;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"YouthRise could not load progress: {exception.Message}");
                return false;
            }
        }

        public static void Clear()
        {
            try
            {
                string path = GetPath();
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"YouthRise could not clear progress: {exception.Message}");
            }
        }

        private static string GetPath()
        {
            return Path.Combine(Application.persistentDataPath, "YouthRise", SaveFileName);
        }
    }
}
