using UnityEngine.TestTools.TestRunner;

namespace UnityEngine.TestRunner.Utils
{
    internal class RuntimeTestRunCallbackListener : TestRunCallbackListener
    {
        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var controller = PlaymodeTestsController.GetController();
            controller.runStartedEvent.AddListener(RunStarted);
            controller.testStartedEvent.AddListener(TestStarted);
            controller.testFinishedEvent.AddListener(TestFinished);
            controller.runFinishedEvent.AddListener(RunFinished);
        }
    }
}
