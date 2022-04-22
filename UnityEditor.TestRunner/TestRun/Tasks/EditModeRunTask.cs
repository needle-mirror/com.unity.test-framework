using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Internal.Filters;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions.Filters;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class EditModeRunTask : TestTaskBase
    {
        public EditModeRunTask()
        {
            SupportsResumingEnumerator = true;
            RunOnCancel = true;
            RerunAfterResume = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.taskInfoStack.Peek().taskMode == TaskMode.Canceled)
            {
                var runner = testJobData.editModeRunner;
                if (runner == null)
                {
                    yield break;
                }
                runner.OnRunCancel();

                while (testJobData.editModeRunner != null && !testJobData.editModeRunner.RunFinished)
                {
                    runner.TestConsumer(testJobData.testRunnerStateSerializer);
                    if (!testJobData.executionSettings.runSynchronously)
                    {
                        yield return null;
                    }
                }

                yield break;
            }
            var filter = testJobData.testFilter;
            var nonPlayModeFilter = new AndFilterExtended(filter, new RequiresPlayModeFilter(false));

            if (testJobData.taskInfoStack.Peek().taskMode == TaskMode.Resume)
            {
                var runner = testJobData.editModeRunner;
                if (runner == null)
                {
                    yield break;
                }

                if (testJobData.testTree == null)
                {
                    throw new Exception("Test tree is required to resume test execution.");
                }

                runner.Resume(nonPlayModeFilter, testJobData.testTree,
                    testJobData.TestStartedEvent, testJobData.TestFinishedEvent, testJobData.Context);

                yield break;
            }

            var editModeRunner = ScriptableObject.CreateInstance<EditModeRunner>();
            editModeRunner.UnityTestAssemblyRunnerFactory = new UnityTestAssemblyRunnerFactory();

            editModeRunner.Init(nonPlayModeFilter, testJobData.executionSettings.runSynchronously, testJobData.testTree,
                testJobData.TestStartedEvent, testJobData.TestFinishedEvent, testJobData.Context);

            testJobData.editModeRunner = editModeRunner;

            while (testJobData.editModeRunner != null && !testJobData.editModeRunner.RunFinished)
            {
                var runner = testJobData.editModeRunner;
                runner.TestConsumer(testJobData.testRunnerStateSerializer);

                if (!testJobData.executionSettings.runSynchronously)
                {
                    yield return null;
                }
            }
        }
    }
}
