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
