using System;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions;

namespace UnityEngine.TestTools.TestRunner
{
    [Serializable]
    internal class TestResultSerializer
    {
        private static readonly BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public |
            BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        [SerializeField] public string id;

        [SerializeField] public string fullName;

        [SerializeField] public double duration;

        [SerializeField] public string label;

        [SerializeField] public string message;

        [SerializeField] public string output;

        [SerializeField] public string site;

        [SerializeField] public string stacktrace;

        [SerializeField] public double startTimeAO;

        [SerializeField] public double endTimeAO;

        [SerializeField] public string status;

        [SerializeField] public string uniqueName;

        public static TestResultSerializer MakeFromTestResult(ITestResult result)
        {
            var wrapper = new TestResultSerializer();
            wrapper.id = result.Test.Id;
            wrapper.fullName = result.FullName;
            wrapper.status = result.ResultState.Status.ToString();
            wrapper.label = result.ResultState.Label;
            wrapper.site = result.ResultState.Site.ToString();
            wrapper.output = result.Output;
            wrapper.duration = result.Duration;
            wrapper.stacktrace = result.StackTrace;
            wrapper.message = result.Message;
            wrapper.startTimeAO = result.StartTime.ToOADate();
            wrapper.endTimeAO = result.EndTime.ToOADate();
            wrapper.uniqueName = result.Test.GetUniqueName();
            return wrapper;
        }

        public void RestoreTestResult(TestResult result)
        {
            var resultState = new ResultState((TestStatus)Enum.Parse(typeof(TestStatus), status), label,
                (FailureSite)Enum.Parse(typeof(FailureSite), site));
            var baseType = result.GetType().BaseType;
            baseType.GetField("_resultState", flags).SetValue(result, resultState);
            baseType.GetField("_output", flags).SetValue(result, new StringBuilder(output));
            baseType.GetField("_duration", flags).SetValue(result, duration);
            baseType.GetField("_message", flags).SetValue(result, message);
            baseType.GetField("_stackTrace", flags).SetValue(result, stacktrace);
            baseType.GetProperty("StartTime", flags)
                .SetValue(result, DateTime.FromOADate(startTimeAO), null);
            baseType.GetProperty("EndTime", flags)
                .SetValue(result, DateTime.FromOADate(endTimeAO), null);
        }

        public bool IsPassed()
        {
            return status == TestStatus.Passed.ToString();
        }

        public static TestResultSerializer MergeResults(TestResultSerializer first, TestResultSerializer second)
        {
            var merged = new TestResultSerializer();
            merged.id = first.id;
            merged.fullName = first.fullName;
            merged.status = MergeResultStatus(first.status, second.status);
            merged.label = merged.status == first.status ? first.label : second.label;
            merged.site = merged.status == first.status ? first.site : second.site;
            merged.output = MergeTextBlocks(first.output, second.output);
            merged.duration = first.duration + second.duration;
            merged.stacktrace = MergeTextBlocks(first.stacktrace, second.stacktrace);
            merged.message = MergeTextBlocks(first.message, second.message);
            merged.startTimeAO = first.startTimeAO;
            merged.uniqueName = first.uniqueName;
            return merged;
        }

        private static string MergeTextBlocks(params string[] strings)
        {
            return string.Join(Environment.NewLine, strings.Where(s => !string.IsNullOrEmpty(s)));
        }

        private static string MergeResultStatus(string first, string second)
        {
            if (first == TestStatus.Failed.ToString() || second == TestStatus.Failed.ToString())
            {
                return TestStatus.Failed.ToString();
            }

            if (first == TestStatus.Inconclusive.ToString() || second == TestStatus.Inconclusive.ToString())
            {
                return TestStatus.Inconclusive.ToString();
            }

            if (first == TestStatus.Passed.ToString() || second == TestStatus.Passed.ToString())
            {
                return TestStatus.Passed.ToString();
            }

            return TestStatus.Skipped.ToString();
        }
    }
}
