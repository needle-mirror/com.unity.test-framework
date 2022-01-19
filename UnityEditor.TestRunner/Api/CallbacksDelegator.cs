using System;
using System.Linq;
using System.Text;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestRunner.TestLaunchers;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal class CallbacksDelegator : ICallbacksDelegator
    {
        private static CallbacksDelegator s_instance;
        public static CallbacksDelegator instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new CallbacksDelegator(CallbacksHolder.instance.GetAll, new TestAdaptorFactory());
                }
                return s_instance;
            }
        }

        private readonly Func<ICallbacks[]> m_CallbacksProvider;
        private readonly ITestAdaptorFactory m_AdaptorFactory;

        // Note that in the event of a domain reload the filter is not reapplied and will be null
        private ITestFilter m_TestRunFilter;

        public CallbacksDelegator(Func<ICallbacks[]> callbacksProvider, ITestAdaptorFactory adaptorFactory)
        {
            m_CallbacksProvider = callbacksProvider;
            m_AdaptorFactory = adaptorFactory;
        }

        public void RunStarted(ITest testsToRun)
        {
            m_AdaptorFactory.ClearResultsCache();
            var testRunnerTestsToRun = m_AdaptorFactory.Create(testsToRun, m_TestRunFilter);
            RunStarted(testRunnerTestsToRun);
        }

        public void RunStarted(ITestAdaptor testRunnerTestsToRun)
        {
            TryInvokeAllCallbacks(callbacks => callbacks.RunStarted(testRunnerTestsToRun));
        }

        public void RunFinished(ITestResult testResults)
        {
            var testResult = m_AdaptorFactory.Create(testResults);
            RunFinished(testResult);
        }

        public void RunFinished(ITestResultAdaptor testResult)
        {
            TryInvokeAllCallbacks(callbacks => callbacks.RunFinished(testResult));
        }

        public void RunFailed(string failureMessage)
        {
            Debug.LogError(failureMessage);
            TryInvokeAllCallbacks(callbacks =>
            {
                var errorCallback = callbacks as IErrorCallbacks;
                if (errorCallback != null)
                {
                    errorCallback.OnError(failureMessage);
                }
            });
        }

        public void TestStarted(ITest test)
        {
            var testRunnerTest = m_AdaptorFactory.Create(test);
            TestStarted(testRunnerTest);
        }

        public void TestStarted(ITestAdaptor testRunnerTest)
        {
            TryInvokeAllCallbacks(callbacks => callbacks.TestStarted(testRunnerTest));
        }

        public void TestFinished(ITestResult result)
        {
            var testResult = m_AdaptorFactory.Create(result);
            TestFinished(testResult);
        }

        public void TestFinished(ITestResultAdaptor testResult)
        {
            TryInvokeAllCallbacks(callbacks => callbacks.TestFinished(testResult));
        }

        public void TestTreeRebuild(ITest test)
        {
            using (new ProfilerMarker(nameof(TestTreeRebuild)).Auto())
            {
                m_AdaptorFactory.ClearTestsCache();
                ITestAdaptor testAdaptor;
                using (new ProfilerMarker("CreateTestAdaptors").Auto())
                    testAdaptor = m_AdaptorFactory.Create(test);
                TryInvokeAllCallbacks(callbacks =>
                {
                    var rebuildCallbacks = callbacks as ITestTreeRebuildCallbacks;
                    if (rebuildCallbacks != null)
                    {
                        rebuildCallbacks.TestTreeRebuild(testAdaptor);
                    }
                });
            }
        }

        public void SetTestRunFilter(ITestFilter filter)
        {
            m_TestRunFilter = filter;
        }

        private void TryInvokeAllCallbacks(Action<ICallbacks> callbackAction)
        {
            foreach (var testRunnerApiCallback in m_CallbacksProvider())
            {
                try
                {
                    callbackAction(testRunnerApiCallback);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private static T Deserialize<T>(byte[] data)
        {
            return JsonUtility.FromJson<T>(Encoding.UTF8.GetString(data));
        }

        public void ClearTestResultCache()
        {
            m_AdaptorFactory.ClearResultsCache();
        }
    }
}
