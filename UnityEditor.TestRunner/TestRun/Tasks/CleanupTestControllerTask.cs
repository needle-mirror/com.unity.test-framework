using System.Collections;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class CleanupTestControllerTask : TestTaskBase
    {
        public CleanupTestControllerTask()
        {
            RunOnCancel = true;
            RunOnError = ErrorRunMode.RunAlways;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (!testJobData.HasPlaymodeTestsController)
            {
                yield break;
            }

            PlaymodeTestsController.ActiveController.Cleanup();
            testJobData.HasPlaymodeTestsController = false;
        }
    }
}
