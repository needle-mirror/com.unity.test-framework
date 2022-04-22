using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class RegisterPlayerRunFinishTask : TestTaskBase
    {
        public RegisterPlayerRunFinishTask()
        {
            RerunAfterResume = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            testJobData.RunFinishedEvent.AddListener((t) =>
            {
                testJobData.PlayerHasFinished = true;
            });

            yield return null;
        }
    }
}
