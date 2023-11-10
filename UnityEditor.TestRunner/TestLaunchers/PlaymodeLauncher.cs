using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestRunner.Utils;
using UnityEngine.TestTools.TestRunner;
using UnityEngine.TestTools.TestRunner.Callbacks;

namespace UnityEditor.TestTools.TestRunner
{
    internal class PlaymodeLauncher : RuntimeTestLauncherBase
    {
        public static bool IsRunning; // This flag is being used by the graphics test framework to detect EditMode/PlayMode.
        public static bool HasFinished;
        private Scene m_Scene;
        private bool m_IsTestSetupPerformed;
        private ITestFilter testFilter;
        private PlaymodeTestsController m_Runner;

        [SerializeField]
        private List<Type> m_EventHandlers = new List<Type>();

        private string m_SceneName;

        public PlaymodeLauncher(PlaymodeTestsControllerSettings settings, string sceneName, Scene scene, PlaymodeTestsController runner) : base(settings)
        {
            m_SceneName = sceneName;
            m_Scene = scene;
            m_Runner = runner;
        }

        public override void Run()
        {
            HasFinished = false;
            IsRunning = true;
            m_Settings.consoleErrorPaused = ConsoleWindow.GetConsoleErrorPause();
            m_Settings.runInBackgroundValue = Application.runInBackground;
            ConsoleWindow.SetConsoleErrorPause(false);
            Application.runInBackground = true;

            CreateBootstrapScene(m_SceneName, m_Scene, m_Runner, runner =>
            {
                runner.AddEventHandlerMonoBehaviour<PlayModeRunnerCallback>();
                runner.AddEventHandlerScriptableObject<TestRunnerCallback>();

                foreach (var eventHandler in m_EventHandlers)
                {
                    var obj = ScriptableObject.CreateInstance(eventHandler);
                    runner.AddEventHandlerScriptableObject(obj as ITestRunnerListener);
                }
            });

            if (m_Settings.sceneBased)
            {
                var newListOfScenes =
                    new List<EditorBuildSettingsScene> {new EditorBuildSettingsScene(m_SceneName, true)};
                newListOfScenes.AddRange(EditorBuildSettings.scenes);
                EditorBuildSettings.scenes = newListOfScenes.ToArray();
            }

            EditorApplication.update += UpdateCallback;
        }

        public void UpdateCallback()
        {
            try
            {
                if (m_IsTestSetupPerformed)
                {
                    if (m_Scene.IsValid())
                        SceneManager.SetActiveScene(m_Scene);
                    EditorApplication.update -= UpdateCallback;
                    EditorApplication.isPlaying = true;
                }
                else
                {
                    testFilter = m_Settings.BuildNUnitFilter();
                    var runner = LoadTests(testFilter);

                    var exceptionThrown = ExecutePreBuildSetupMethods(runner.LoadedTest, testFilter);
                    if (exceptionThrown)
                    {
                        EditorApplication.update -= UpdateCallback;
                        var controller = PlaymodeTestsController.GetController();
                        ReopenOriginalScene(controller);
                        CallbacksDelegator.instance.RunFailed("Run Failed: One or more errors in a prebuild setup. See the editor log for details.");
                        HasFinished = true;
                        IsRunning = false;
                        return;
                    }
                    m_IsTestSetupPerformed = true;
                }
            }
            catch (Exception ex)
            {
                EditorApplication.update -= UpdateCallback;
                CallbacksDelegator.instance.RunFailed(ex.Message);
                HasFinished = true;
                IsRunning = false;
                throw;
            }
        }

        [InitializeOnLoad]
        public class BackgroundWatcher
        {
            static BackgroundWatcher()
            {
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            }

            private static void OnPlayModeStateChanged(PlayModeStateChange state)
            {
                if (!PlaymodeTestsController.IsControllerOnScene())
                    return;
                var runner = PlaymodeTestsController.GetController();
                if (runner == null)
                    return;
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    HasFinished = true;
                    if (runner.m_Runner != null)
                    {
                        ExecutePostBuildCleanupMethods(runner.m_Runner.LoadedTest, runner.settings.BuildNUnitFilter(), Application.platform);
                    }
                }
                else if (state == PlayModeStateChange.EnteredEditMode)
                {
                    ConsoleWindow.SetConsoleErrorPause(runner.settings.consoleErrorPaused);
                    Application.runInBackground = runner.settings.runInBackgroundValue;
                    //reopen the original scene once we exit playmode
                    ReopenOriginalScene(runner);
                }
            }
        }

        protected static void ReopenOriginalScene(PlaymodeTestsController runner)
        {
            ReopenOriginalScene(runner.settings.originalScene);
        }

        public void AddEventHandler<T>() where T : ScriptableObject, ITestRunnerListener
        {
            m_EventHandlers.Add(typeof(T));
        }
    }
}
