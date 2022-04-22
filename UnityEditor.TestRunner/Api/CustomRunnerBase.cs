using System;
using System.Linq;

namespace UnityEditor.TestTools.TestRunner.Api
{
    public abstract class CustomRunnerBase
    {
        internal string name;
        internal ICallbacksDelegator callbacksDelegator { get; set; } = CallbacksDelegator.instance;

        protected CustomRunnerBase(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name), "Custom runner name must not be null.");
            }

            if (name == string.Empty || name.All(char.IsWhiteSpace))
            {
                throw new ArgumentException("Custom runner name must not be empty or whitespace.", nameof(name));
            }

            if (name.Contains(Environment.NewLine))
            {
                throw new ArgumentException("Custom runner name must be a single line string.", nameof(name));
            }

            this.name = name;
        }

        public abstract void Execute(Filter[] filters, bool runSynchronously);

        public abstract void GetTestList(Action<ITestAdaptor> testListCallback);

        protected void ReportRunStarted(ITestAdaptor testsToRun)
        {
            callbacksDelegator.RunStarted(testsToRun);
        }

        protected void ReportRunFinished(ITestResultAdaptor testResults)
        {
            IsDone = true;
            callbacksDelegator.RunFinished(testResults);
        }

        protected void ReportTestStarted(ITestAdaptor test)
        {
            callbacksDelegator.TestStarted(test);
        }

        protected void ReportTestFinished(ITestResultAdaptor result)
        {
            callbacksDelegator.TestFinished(result);
        }

        protected void ReportRunFailed(string failureMessage)
        {
            IsDone = true;
            callbacksDelegator.RunFailed(failureMessage);
        }

        internal bool IsDone { get; private set; }
    }
}
