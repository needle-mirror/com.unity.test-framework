using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Events
{
    internal class RunFinishedInvocationEvent : TestTaskBase
    {
        internal Action _clearTestResultFactoryCache = () => CallbacksDelegator.instance.ClearTestResultCache();
        public override IEnumerator Execute(TestJobData testJobData)
        {
            _clearTestResultFactoryCache();
            var testResults = RestoreTestResult((Test)testJobData.testTree, testJobData.TestResults);
            testJobData.RunFinishedEvent.Invoke(testResults);
            yield break;
        }

        private ITestResult RestoreTestResult(Test test, List<TestResultSerializer> testResultSerializers)
        {
            var uniqueName = test.GetUniqueName();
            var serializedResults = testResultSerializers.Where(r => r.uniqueName == uniqueName).ToArray();
            TestResultSerializer serializedResult = null;
            if (serializedResults.Length == 0)
            {
                return null;
            }
            else if (serializedResults.Length == 1)
            {
                serializedResult = serializedResults[0];
            }
            else if (serializedResults.Length == 2)
            {
                serializedResult = TestResultSerializer.MergeResults(serializedResults[0], serializedResults[1]);
            }
            else
            {
                throw new Exception($"Could not merge {serializedResults.Length} results matching {uniqueName}.");
            }

            var result = test.MakeTestResult();
            serializedResult.RestoreTestResult(result);

            foreach (var childTest in test.Tests)
            {
                var childResult = RestoreTestResult((Test)childTest, testResultSerializers);
                if (childResult != null && result is TestSuiteResult)
                {
                    ((TestSuiteResult)result).AddResult(childResult);
                }
            }
            return result;
        }
    }
}
