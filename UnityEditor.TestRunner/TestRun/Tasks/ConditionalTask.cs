using System;
using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class ConditionalTask : TestTaskBase
    {
        private TestTaskBase m_Task;
        private Func<TestJobData, bool> m_Condition;
        public ConditionalTask(TestTaskBase task, Func<TestJobData, bool> condition)
        {
            m_Task = task;
            m_Condition = condition;
            RerunAfterResume = task.RerunAfterResume;
            RunOnCancel = task.RunOnCancel;
            RunOnError = task.RunOnError;
            SupportsResumingEnumerator = task.SupportsResumingEnumerator;
            RerunAfterEnteredEditMode = task.RerunAfterEnteredEditMode;
        }

        public override string GetName()
        {
            return m_Task.GetName();
        }

        public bool ConditionFulfilled(TestJobData testJobData)
        {
            return m_Condition(testJobData);
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            return ConditionFulfilled(testJobData) ? m_Task.Execute(testJobData) : NoOperation();
        }

        private static IEnumerator NoOperation()
        {
            yield break;
        }
    }
}
