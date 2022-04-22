using System.Collections;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class BuildAndRunPlayerTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            var builder = TestRunnerApi.GetPlayerBuilderFromName(testJobData.executionSettings.playerBuilderName);
            var enumerator = builder.BuildAndRun(testJobData.executionSettings, testJobData.PlayerBuildOptions);

            while (enumerator.MoveNext())
            {
                yield return null;
            }
        }
    }
}
