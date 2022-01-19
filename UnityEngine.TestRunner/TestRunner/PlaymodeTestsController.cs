using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.NUnitExtensions;
using UnityEngine.TestTools.Utils;

namespace UnityEngine.TestTools.TestRunner
{
    [Serializable]
    [AddComponentMenu("")]
    internal class PlaymodeTestsController : MonoBehaviour
    {
        public static bool RunFinished;
        private IEnumerator m_TestSteps;

        public static PlaymodeTestsController ActiveController { get; set; }

        [SerializeField]
        private List<string> m_AssembliesWithTests;
        public List<string> AssembliesWithTests
        {
            get
            {
                return m_AssembliesWithTests;
            }
            set
            {
                m_AssembliesWithTests = value;
            }
        }

        [SerializeField]
        internal TestStartedEvent testStartedEvent = new TestStartedEvent();
        [SerializeField]
        internal TestFinishedEvent testFinishedEvent = new TestFinishedEvent();
        [SerializeField]
        internal RunStartedEvent runStartedEvent = new RunStartedEvent();
        [SerializeField]
        internal RunFinishedEvent runFinishedEvent = new RunFinishedEvent();
        [SerializeField]
        internal ScriptableObject[] includedObjects;

        internal const string kPlaymodeTestControllerName = "Code-based tests runner";

        [SerializeField]
        public PlaymodeTestsControllerSettings settings = new PlaymodeTestsControllerSettings();

        internal UnityTestAssemblyRunner m_Runner;

        public IEnumerator Start()
        {
            ActiveController = this;
            //Skip 2 frame because Unity.
            yield return null;
            yield return null;
            StartCoroutine(Run());
        }

        internal static bool IsControllerOnScene()
        {
            return GameObject.Find(kPlaymodeTestControllerName) != null;
        }

        internal static PlaymodeTestsController GetController()
        {
            return GameObject.Find(kPlaymodeTestControllerName).GetComponent<PlaymodeTestsController>();
        }

        public IEnumerator TestRunnerCoroutine()
        {
            while (m_TestSteps.MoveNext())
            {
                yield return m_TestSteps.Current;
            }

            if (m_Runner.IsTestComplete)
            {
                runFinishedEvent.Invoke(m_Runner.Result);

                RunFinished = true;
                yield return null;
            }
        }

        public IEnumerator Run()
        {
            CoroutineTestWorkItem.monoBehaviourCoroutineRunner = this;
            gameObject.hideFlags |= HideFlags.DontSave;

            var testListUtil = new PlayerTestAssemblyProvider(new AssemblyLoadProxy(), m_AssembliesWithTests);
            m_Runner = new UnityTestAssemblyRunner(new UnityTestAssemblyBuilder(), new PlaymodeWorkItemFactory(), new UnityTestExecutionContext());

            var assemblies = testListUtil.GetUserAssemblies()
                .Select(a => new AssemblyWithPlatform(a, TestPlatform.PlayMode))
                .ToArray();

            var loadedTests = m_Runner.Load(assemblies);
            loadedTests.ParseForNameDuplicates();
            runStartedEvent.Invoke(m_Runner.LoadedTest);

            var testListenerWrapper = new TestListenerWrapper(testStartedEvent, testFinishedEvent);
            m_TestSteps = m_Runner.Run(testListenerWrapper, settings.BuildNUnitFilter()).GetEnumerator();

            yield return TestRunnerCoroutine();
        }

        public void Cleanup()
        {
            if (m_Runner != null)
            {
                m_Runner.StopRun();
                m_Runner = null;
            }
            if (Application.isEditor)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
            }
        }

        public static void TryCleanup()
        {
            var controller = GameObject.Find(kPlaymodeTestControllerName);
            if (controller != null)
            {
                controller.GetComponent<PlaymodeTestsController>().Cleanup();
            }
        }
    }
}
