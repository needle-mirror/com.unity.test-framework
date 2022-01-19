using System;
using System.Collections;
using NUnit.Framework.Internal.Filters;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions.Filters;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class PlayModeRunTask : TestTaskBase
    {
        private UnityTestAssemblyRunner m_Runner;

        public PlayModeRunTask()
        {
            SupportsResumingEnumerator = true;
            RunOnCancel = true;
            RerunAfterResume = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            var taskMode = testJobData.taskInfoStack.Peek().taskMode;
            if (taskMode == TaskMode.Canceled)
            {
                if (m_Runner != null)
                {
                    m_Runner.StopRun();
                }
                yield break;
            }

            if (!Application.isPlaying)
            {
                throw new Exception($"Application must be in playmode to run tests in {nameof(PlayModeRunTask)}.");
            }

            m_Runner = new UnityTestAssemblyRunner(new UnityTestAssemblyBuilder(), new PlaymodeWorkItemFactory(), new UnityTestExecutionContext());
            m_Runner.LoadTestTree(testJobData.testTree);

            if (taskMode == TaskMode.Resume)
            {
                yield break;
            }

            var testListenerWrapper = new TestListenerWrapper(testJobData.TestStartedEvent, testJobData.TestFinishedEvent);
            var playModeOnlyFilter = new AndFilterExtended(testJobData.testFilter, new RequiresPlayModeFilter(true));
            var steps = m_Runner.Run(testListenerWrapper, playModeOnlyFilter).GetEnumerator();

            var coroutineRunnerObject = new GameObject("tests runner");
            var coroutineRunner = coroutineRunnerObject.AddComponent<CoroutineRunner>();
            CoroutineTestWorkItem.monoBehaviourCoroutineRunner = coroutineRunner;
            coroutineRunner.Run(steps);

            while (m_Runner.IsTestRunning)
            {
                yield return null;
            }
        }

        internal class CoroutineRunner : MonoBehaviour
        {
            public void Run(IEnumerator steps)
            {
                gameObject.hideFlags |= HideFlags.DontSave;
                StartCoroutine(Unpack(steps));
            }

            private IEnumerator Unpack(IEnumerator steps)
            {
                yield return null;
                yield return null;
                while (steps.MoveNext())
                {
                    yield return steps.Current;
                }
            }
        }
    }
}
