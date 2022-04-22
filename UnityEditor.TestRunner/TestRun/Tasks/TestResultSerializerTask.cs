using System.Collections;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class TestResultSerializerTask : TestTaskBase
    {
        public TestResultSerializerTask()
        {
            RerunAfterResume = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            testJobData.TestFinishedEvent.AddListener(result =>
            {
                testJobData.TestResults.Add(TestResultSerializer.MakeFromTestResult(result));
            });
            yield break;
        }
    }
}
