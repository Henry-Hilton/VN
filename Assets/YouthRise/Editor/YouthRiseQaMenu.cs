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

        [MenuItem("YouthRise/QA/Start Chapter 4", false, 17)]
        private static void StartChapterFour()
        {
            InvokeButton("Start Chapter 4");
        }

        [MenuItem("YouthRise/QA/Start Chapter 5", false, 18)]
        private static void StartChapterFive() => InvokeButton("Start Chapter 5");

        [MenuItem("YouthRise/QA/Open Financial Guides", false, 19)]
        private static void OpenFinancialGuides() => InvokeButton("Financial Tab");

        [MenuItem("YouthRise/QA/Continue Saved Game", false, 20)]
        private static void ContinueSavedGame() => InvokeButton("Continue");

        [MenuItem("YouthRise/QA/Choose Second Option", false, 21)]
        private static void ChooseSecondOption() => InvokeButton("Choice 2");

        [MenuItem("YouthRise/QA/Choose Third Option", false, 22)]
        private static void ChooseThirdOption() => InvokeButton("Choice 3");

        [MenuItem("YouthRise/QA/Return From Safe Zone", false, 23)]
        private static void ReturnFromSafeZone() => InvokeButton("Close");

        [MenuItem("YouthRise/QA/Open Safe Zone", false, 24)]
        private static void OpenSafeZone() => InvokeButton("Safe Zone");

        [MenuItem("YouthRise/QA/Return From Completion", false, 25)]
        private static void ReturnFromCompletion() => InvokeButton("Back to Menu");

        private static void InvokeButton(string objectName)
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("YouthRise QA controls are available in Play mode.");
                return;
            }

            Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude);

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
