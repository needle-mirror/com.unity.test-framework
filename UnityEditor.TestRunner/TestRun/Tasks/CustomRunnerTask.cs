using System;
using System.Collections;
using System.Linq;
using UnityEditor.TestTools.TestRunner.Api;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class CustomRunnerTask : TestTaskBase
    {
        internal ICustomRunnerHolder customRunnerHolder = CustomRunnerHolder.instance;

        public override IEnumerator Execute(TestJobData testJobData)
        {
            var customRunnerName = testJobData.executionSettings.customRunnerName;
            var customRunner = customRunnerHolder.Get(customRunnerName);
            if (customRunner == null)
            {
                throw new Exception($"Could not find a custom runner matching '{customRunnerName}'.");
            }

            customRunner.Execute(testJobData.executionSettings.filters, testJobData.executionSettings.runSynchronously);
            yield return null;
            while (!customRunner.IsDone)
            {
                yield return null;
            }
        }
    }
}
