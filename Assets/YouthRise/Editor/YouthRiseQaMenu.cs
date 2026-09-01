#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace YouthRise.EditorTools
{
    public static class YouthRiseQaMenu
    {
        [MenuItem("YouthRise/QA/Start Chapter", false, 10)]
        private static void StartChapter()
        {
            InvokeButton("Start");
        }

        [MenuItem("YouthRise/QA/Continue Story", false, 11)]
        private static void ContinueStory()
        {
            InvokeButton("Continue Story");
        }

        [MenuItem("YouthRise/QA/Choose First Option", false, 12)]
        private static void ChooseFirstOption()
        {
            InvokeButton("Choice 1");
        }

        [MenuItem("YouthRise/QA/Start Chapter 2", false, 13)]
        private static void StartChapterTwo()
        {
            InvokeButton("Start Chapter 2");
        }

        [MenuItem("YouthRise/QA/Continue From Completion", false, 14)]
        private static void ContinueFromCompletion()
        {
            InvokeButton("Completion Primary");
        }

        [MenuItem("YouthRise/QA/Open Safe Zone Articles", false, 15)]
        private static void OpenSafeZoneArticles()
        {
            InvokeButton("Article Tab");
        }

        [MenuItem("YouthRise/QA/Start Chapter 3", false, 16)]
        private static void StartChapterThree()
        {
            InvokeButton("Start Chapter 3");
        }

        private static void InvokeButton(string objectName)
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("YouthRise QA controls are available in Play mode.");
                return;
            }

            Button[] buttons = Object.FindObjectsByType<Button>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            foreach (Button button in buttons)
            {
                if (!button.interactable || button.gameObject.name != objectName)
                    continue;

                button.onClick.Invoke();
                return;
            }

            Debug.LogWarning($"YouthRise QA could not find an interactable '{objectName}' button.");
        }
    }
}
#endif
