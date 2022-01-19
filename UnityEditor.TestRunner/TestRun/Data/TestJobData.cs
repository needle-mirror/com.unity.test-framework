using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    [Serializable]
    internal class TestJobData : ISerializationCallbackReceiver
    {
        [SerializeField]
        public string guid;

        [SerializeField]
        public string startTime;

        [NonSerialized]
        public Stack<TaskInfo> taskInfoStack = new Stack<TaskInfo>();

        [SerializeField]
        private TaskInfo[] savedTaskInfoStack;

        [SerializeField]
        public bool isRunning;

        [SerializeField]
        public ExecutionSettings executionSettings;

        [SerializeField]
        public RunProgress runProgress = new RunProgress();

        [SerializeField]
        public string[] existingFiles;

        [SerializeField]
        public int undoGroup = -1;

        [SerializeField]
        public EditModeRunner editModeRunner;

        [SerializeField]
        public BeforeAfterTestCommandState setUpTearDownState;

        [SerializeField]
        public BeforeAfterTestCommandState outerUnityTestActionState;

        [SerializeField]
        public TestRunnerStateSerializer testRunnerStateSerializer;

        [NonSerialized]
        public bool isHandledByRunner;

        [SerializeField]
        public SceneSetup[] SceneSetup;

        [SerializeField]
        public TestProgress testProgress;

        [NonSerialized]
        public TestTaskBase[] Tasks;

        [NonSerialized]
        public ITest testTree;

        [NonSerialized]
        public ITest currentPlayerTest;

        [NonSerialized]
        public Stack<ITestResult> childPlayerTestResults;

        [SerializeField]
        public bool hasTestThatRequiresPlayMode;
        [SerializeField]
        public bool hasTestThatDoesNotRequiresPlayMode;

        [NonSerialized]
        public ITestFilter testFilter;
        [NonSerialized]
        public ITestFilter requirePlayModeFilter;
        [NonSerialized]
        public ITestFilter doesNotRequirePlayModeFilter;

        [NonSerialized]
        public TestStartedEvent TestStartedEvent;
        [NonSerialized]
        public TestFinishedEvent TestFinishedEvent;
        [NonSerialized]
        public RunStartedEvent RunStartedEvent;
        [NonSerialized]
        public RunFinishedEvent RunFinishedEvent;

        [NonSerialized]
        public UnityTestExecutionContext Context;

        [NonSerialized]
        public ConstructDelegator ConstructDelegator;

        [SerializeField]
        public string OriginalScenePath;

        [SerializeField]
        public Scene InitTestScene;

        [SerializeField]
        public string InitTestScenePath;

        [SerializeField]
        public BuildPlayerOptions PlayerBuildOptions;

        [SerializeField]
        public bool HasPlaymodeTestsController;

        [SerializeField]
        public PlaymodeTestsControllerSettings PlayModeSettings;

        [SerializeField]
        public List<TestResultSerializer> TestResults = new List<TestResultSerializer>();

        [SerializeField]
        public PlatformSpecificSetup PlatformSpecificSetup;

        [NonSerialized]
        public PlayerLauncherContextSettings PlayerLauncherContextSettings;

        [NonSerialized]
        public RuntimePlatform? TargetRuntimePlatform;

        [SerializeField]
        public SavedProjectSettings OriginalProjectSettings;

        [SerializeField]
        public bool PlayerHasFinished;

        public TestJobData(ExecutionSettings settings)
        {
            guid = Guid.NewGuid().ToString();
            executionSettings = settings;
            isRunning = false;
            startTime = DateTime.Now.ToString("o");
        }

        public void OnBeforeSerialize()
        {
            savedTaskInfoStack = taskInfoStack.ToArray();
        }

        public void OnAfterDeserialize()
        {
            taskInfoStack = new Stack<TaskInfo>(savedTaskInfoStack);
        }

        [Serializable]
        internal class SavedProjectSettings
        {
            public bool runInBackgroundValue;

            public bool consoleErrorPaused;
        }
    }
}
