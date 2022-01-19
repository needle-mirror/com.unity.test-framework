using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.TestRunner.TestLaunchers;
using UnityEditor.TestTools.TestRunner;
using UnityEditor.TestTools.TestRunner.TestRun;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestRunner.Utils;
using UnityEngine.TestTools.TestRunner;
using UnityEngine.TestTools.TestRunner.Callbacks;

namespace TestRun.Tasks.Player
{
    internal class PreparePlayerSceneTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            var remotePlayerLogController = RemotePlayerLogController.instance;
            remotePlayerLogController.hideFlags = HideFlags.HideAndDontSave;

            var executionSettings = testJobData.executionSettings;
            var settings = PlaymodeTestsControllerSettings.CreateRunnerSettings(executionSettings.filters.Select(filter => filter.ToRuntimeTestRunnerFilter(executionSettings.runSynchronously)).ToArray());

            PrepareScene(testJobData.InitTestScene, testJobData.InitTestScenePath, settings);
            yield return null;
        }

        private static void PrepareScene(Scene scene, string scenePath, PlaymodeTestsControllerSettings settings)
        {
            var runner = GameObject.Find(PlaymodeTestsController.kPlaymodeTestControllerName).GetComponent<PlaymodeTestsController>();
            AddEventHandlerMonoBehaviour<PlayModeRunnerCallback>(runner);
            runner.settings = settings;
            var commandLineArgs = Environment.GetCommandLineArgs();
            var remoteTestResultSender = AddEventHandlerMonoBehaviour<RemoteTestResultSender>(runner);
            remoteTestResultSender.ReportBackToEditor =
                !commandLineArgs.Contains("-doNotReportTestResultsBackToEditor");

            runner.includedObjects = new ScriptableObject[]
            {ScriptableObject.CreateInstance<RuntimeTestRunCallbackListener>()};
            SaveScene(scene, scenePath);
        }

        private static void SaveScene(Scene scene, string scenePath)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, scenePath, false);
        }

        private static T AddEventHandlerMonoBehaviour<T>(PlaymodeTestsController controller) where T : MonoBehaviour, ITestRunnerListener
        {
            var eventHandler = controller.gameObject.AddComponent<T>();
            UnityEventTools.AddPersistentListener(controller.testStartedEvent, eventHandler.TestStarted);
            UnityEventTools.AddPersistentListener(controller.testFinishedEvent, eventHandler.TestFinished);
            return eventHandler;
        }
    }
}
