using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class CleanupProjectParametersTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.PlayerLauncherContextSettings != null)
            {
                testJobData.PlayerLauncherContextSettings.Dispose();
            }
            yield return null;
        }
    }
}
