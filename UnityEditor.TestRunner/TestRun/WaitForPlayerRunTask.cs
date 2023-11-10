using System.Collections;
using UnityEditor.TestRunner.TestLaunchers;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    internal class WaitForPlayerRunTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            while (RemoteTestRunController.instance.isRunning)
            {
                yield return null;
            }
        }
    }
}
