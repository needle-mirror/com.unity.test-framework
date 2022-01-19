using NUnit.Framework.Interfaces;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal interface ICallbacksDelegator
    {
        void RunStarted(ITest testsToRun);
        void RunStarted(ITestAdaptor testsToRun);
        void RunFinished(ITestResult testResults);
        void RunFinished(ITestResultAdaptor testResults);
        void RunFailed(string failureMessage);
        void TestStarted(ITest test);
        void TestStarted(ITestAdaptor test);
        void TestFinished(ITestResult result);
        void TestFinished(ITestResultAdaptor result);
        void TestTreeRebuild(ITest test);
        void SetTestRunFilter(ITestFilter filter);
        void ClearTestResultCache();
    }
}
