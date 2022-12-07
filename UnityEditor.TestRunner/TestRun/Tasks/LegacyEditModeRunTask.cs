using System;
using System.Collections;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class LegacyEditModeRunTask : TestTaskBase
    {
        public LegacyEditModeRunTask()
        {
            SupportsResumingEnumerator = true;
        }
        
        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.taskInfoStack.Peek().taskMode == TaskMode.Canceled)
            {
                var runner = testJobData.editModeRunner;
                if (runner != null)
                {
                    runner.OnRunCancel();
                }
                yield break;
            }

            var testLauncher = new EditModeLauncher(testJobData.executionSettings.filters, TestPlatform.EditMode, testJobData.executionSettings.runSynchronously, 
                 testJobData.RunStartedEvent, testJobData.TestStartedEvent, testJobData.TestFinishedEvent,  testJobData.RunFinishedEvent, testJobData.executionSettings.orderedTestNames);
            testJobData.editModeRunner = testLauncher.m_EditModeRunner;
            testLauncher.Run();
          
            while (testJobData.editModeRunner != null && !testJobData.editModeRunner.RunFinished)
            {
                yield return null;
            }
            
        }
    }
}