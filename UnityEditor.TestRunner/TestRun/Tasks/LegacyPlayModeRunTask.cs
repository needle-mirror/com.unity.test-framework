using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class LegacyPlayModeRunTask : TestTaskBase
    {
        public LegacyPlayModeRunTask()
        {
            SupportsResumingEnumerator = true;
        }
        public override IEnumerator Execute(TestJobData testJobData)
        {
            var launcher = new PlaymodeLauncher(testJobData.PlayModeSettings, testJobData.InitTestScenePath, testJobData.InitTestScene, testJobData.PlaymodeTestsController);

            launcher.Run();

            // Saving of the scene causes serialization of the runner, so the events needs to be resubscribed. This is temporary for now.
            // Wait for the active controller
            while (PlaymodeTestsController.ActiveController == null)
            {
                yield return null;
            }

            var controller = PlaymodeTestsController.ActiveController;
            controller.runStartedEvent.AddListener(testJobData.RunStartedEvent.Invoke);
            controller.testStartedEvent.AddListener(testJobData.TestStartedEvent.Invoke);
            controller.testFinishedEvent.AddListener(testJobData.TestFinishedEvent.Invoke);
            controller.runFinishedEvent.AddListener(testJobData.RunFinishedEvent.Invoke);

            controller.RunInfrastructureHasRegistered = true;
            
            while (!PlaymodeLauncher.HasFinished)
            {
                yield return null;
            }
        }
    }
}
