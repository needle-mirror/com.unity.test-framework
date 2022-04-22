using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class SetupProjectParametersTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            testJobData.PlayerLauncherContextSettings = new PlayerLauncherContextSettings(testJobData.executionSettings.overloadTestRunSettings);
            yield return null;
        }
    }
}
