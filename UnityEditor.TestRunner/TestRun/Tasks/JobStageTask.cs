using System.Collections;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class JobStageTask : TestTaskBase
    {
        private string m_Title;

        public JobStageTask(string title)
        {
            m_Title = title;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            testJobData.runProgress.stageName = m_Title;
            yield break;
        }
    }
}
