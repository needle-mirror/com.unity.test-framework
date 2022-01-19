using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Player
{
    internal class PlayerEventDelegatorTask : TestTaskBase
    {
        internal IPlayerCommunication playerCommunication = PlayerCommunication.instance;
        internal Func<ITest, string> identifyTest = (test) => test.GetUniqueName();
        public override IEnumerator Execute(TestJobData testJobData)
        {
            playerCommunication.Init();
            playerCommunication.OnTestStarted.AddListener((testUniqueName) => { OnTestStarted(testJobData, testUniqueName); });
            playerCommunication.OnTestFinished.AddListener((testResult) => { OnTestFinished(testJobData, testResult); });

            yield break;
        }

        private void OnTestStarted(TestJobData testJobData, string testUniqueName)
        {
            if (testJobData.currentPlayerTest == null)
            {
                var firstUniqueName = identifyTest(testJobData.testTree);
                if (firstUniqueName != testUniqueName)
                {
                    throw new PlayerEventDelegatorException($"Expected first received test result from player to be '{firstUniqueName}', got '{testUniqueName}'.");
                }

                testJobData.currentPlayerTest = testJobData.testTree;
                testJobData.childPlayerTestResults = new Stack<ITestResult>();

                testJobData.RunStartedEvent.Invoke(testJobData.currentPlayerTest);
            }
            else
            {
                var matchingChild = testJobData.currentPlayerTest.Tests.FirstOrDefault(test => identifyTest(test) == testUniqueName);
                if (matchingChild == null)
                {
                    throw new PlayerEventDelegatorException(
                        $"Could not match received test name '{testUniqueName}' among any children in '{identifyTest(testJobData.currentPlayerTest)}'. This could be due to a non unique test, class and namespace name.");
                }

                testJobData.currentPlayerTest = matchingChild;
            }

            testJobData.TestStartedEvent.Invoke(testJobData.currentPlayerTest);
        }

        private void OnTestFinished(TestJobData testJobData, TestResultSerializer testResult)
        {
            if (testJobData.currentPlayerTest == null)
            {
                throw new PlayerEventDelegatorException("No current test is set for TestFinished event.");
            }

            var currentTestUniqueName = identifyTest(testJobData.currentPlayerTest);
            if (currentTestUniqueName != testResult.uniqueName)
            {
                throw new PlayerEventDelegatorException(
                    $"Test results received did not match the current test. Should be '{currentTestUniqueName}', got '{testResult.uniqueName}'.");
            }

            var result = ((Test)testJobData.currentPlayerTest).MakeTestResult();
            testResult.RestoreTestResult(result);

            if (result is TestSuiteResult)
            {
                var suiteResult = ((TestSuiteResult)result);
                while (testJobData.childPlayerTestResults.Count > 0 && identifyTest(testJobData.childPlayerTestResults.Peek().Test.Parent) == currentTestUniqueName)
                {
                    suiteResult.AddResult(testJobData.childPlayerTestResults.Pop());
                }
            }

            testJobData.childPlayerTestResults.Push(result);
            testJobData.currentPlayerTest = testJobData.currentPlayerTest.Parent;
            testJobData.TestFinishedEvent.Invoke(result);

            if (result.Test.Parent == null) // The root test object.
            {
                testJobData.RunFinishedEvent.Invoke(result);
            }
        }

        public class PlayerEventDelegatorException : Exception
        {
            public PlayerEventDelegatorException(string msg) : base(msg)
            {
            }
        }
    }
}
