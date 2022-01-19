using System.Collections;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class GenerateContextTask : TestTaskBase
    {
        public GenerateContextTask()
        {
            RerunAfterResume = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.setUpTearDownState?.GetHashCode() == null)
            {
                testJobData.setUpTearDownState = new BeforeAfterTestCommandState();
            }

            if (testJobData.outerUnityTestActionState?.GetHashCode() == null)
            {
                testJobData.outerUnityTestActionState = new BeforeAfterTestCommandState();
            }

            testJobData.Context = new UnityTestExecutionContext()
            {
                SetUpTearDownState = testJobData.setUpTearDownState,
                OuterUnityTestActionState = testJobData.outerUnityTestActionState
            };

            yield break;
        }
    }
}
