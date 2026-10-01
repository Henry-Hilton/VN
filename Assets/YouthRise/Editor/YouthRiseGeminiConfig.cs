#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YouthRise.EditorTools
{
    public static class YouthRiseGeminiConfig
    {
        [MenuItem("YouthRise/Gemini/Open Private Config")]
        private static void Open()
        {
            string path = GeminiLocalSettings.ConfigPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (!File.Exists(path)) File.WriteAllText(path, JsonUtility.ToJson(new GeminiLocalSettings(), true));
            EditorUtility.RevealInFinder(path);
        }
    }
}
#endif
