using System.Collections;
using UnityEditor.TestTools.TestRunner.Api;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Events
{
    internal class RegisterCallbackDelegatorEventsTask : TestTaskBase
    {
        public RegisterCallbackDelegatorEventsTask()
        {
            RerunAfterResume = true;
        }

        internal ICallbacksDelegator ApiCallbacksDelegator = CallbacksDelegator.instance;

        public override IEnumerator Execute(TestJobData testJobData)
        {
            ApiCallbacksDelegator.SetTestRunFilter(testJobData.testFilter);
            testJobData.RunStartedEvent.AddListener(ApiCallbacksDelegator.RunStarted);
            testJobData.TestStartedEvent.AddListener(ApiCallbacksDelegator.TestStarted);
            testJobData.TestFinishedEvent.AddListener(ApiCallbacksDelegator.TestFinished);
            testJobData.RunFinishedEvent.AddListener(ApiCallbacksDelegator.RunFinished);
            yield break;
        }
    }
}
