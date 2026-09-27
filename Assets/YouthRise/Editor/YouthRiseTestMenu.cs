#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace YouthRise.EditorTools
{
    public static class YouthRiseTestMenu
    {
        [MenuItem("YouthRise/QA/Run EditMode Tests", false, 100)]
        private static void Run()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode before running EditMode tests."); return; }
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new ResultWriter());
            api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode, assemblyNames = new[] { "YouthRise.Tests.EditMode" } }));
        }

        private sealed class ResultWriter : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                Directory.CreateDirectory("Temp/YouthRiseQA");
                TestRunnerApi.SaveResultToFile(result, "Temp/YouthRiseQA/editmode-results.xml");
                Debug.Log("YouthRise test result: " + result.ResultState + ". XML: Temp/YouthRiseQA/editmode-results.xml");
                TestRunnerApi.UnregisterTestCallback(this);
            }
        }
    }
}
#endif
