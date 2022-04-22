using System;
using System.Linq;
using System.Text;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Filters;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.Api
{
    /// <summary>
    /// A set of execution settings defining how to run tests, using the <see cref="TestRunnerApi"/>.
    /// </summary>
    [Serializable]
    public class ExecutionSettings : ISerializationCallbackReceiver
    {
        /// <summary>
        /// Creates an instance with a given set of filters, if any.
        /// </summary>
        /// <param name="filtersToExecute">Set of filters</param>
        public ExecutionSettings(params Filter[] filtersToExecute)
        {
            filters = filtersToExecute;
        }

        [SerializeField]
        private BuildTarget m_TargetPlatform;
        [SerializeField]
        private bool m_HasTargetPlatform;

        /// <summary>
        /// An instance of <see cref="ITestRunSettings"/> to set up before running tests on a Player.
        /// </summary>
        // Note: Is not available after serialization
        public ITestRunSettings overloadTestRunSettings;

        [SerializeField]
        internal Filter filter;
        ///<summary>
        ///A collection of <see cref="Filter"/> to execute tests on.
        ///</summary>
        [SerializeField]
        public Filter[] filters;
        /// <summary>
        ///  Note that this is only supported for EditMode tests, and that tests which take multiple frames (i.e. [UnityTest] tests, or tests with [UnitySetUp] or [UnityTearDown] scaffolding) will be filtered out.
        /// </summary>
        /// <returns>If true, the call to Execute() will run tests synchronously, guaranteeing that all tests have finished running by the time the call returns.</returns>
        [SerializeField]
        public bool runSynchronously;
        /// <summary>
        /// The time, in seconds, the editor should wait for heartbeats after starting a test run on a player. This defaults to 10 minutes.
        /// </summary>
        [SerializeField]
        public int playerHeartbeatTimeout = 60 * 10;
        [SerializeField]
        internal string playerBuilderName;
        [SerializeField]
        public string customRunnerName;
        [SerializeField]
        public bool IsBuildOnly { get; set; }

        public string playerSavePath { get; set; }

        /// <summary>
        /// The <see cref="BuildTarget"/> platform to run the test on. If set to null, then the Editor is the target for the tests.
        /// </summary>
        public BuildTarget? targetPlatform
        {
            get { return m_HasTargetPlatform ? (BuildTarget?)m_TargetPlatform : null; }
            set
            {
                {
                    if (value.HasValue)
                    {
                        m_HasTargetPlatform = true;
                        m_TargetPlatform = value.Value;
                    }
                    else
                    {
                        m_HasTargetPlatform = false;
                        m_TargetPlatform = default;
                    }
                }
            }
        }
        /// <summary>
        /// Implementation of ToString() that builds a string composed of the execution settings.
        /// </summary>
        /// <returns>The current execution settings as a string.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{nameof(ExecutionSettings)} with details:");
            stringBuilder.AppendLine($"{nameof(targetPlatform)} = {targetPlatform}");
            stringBuilder.AppendLine($"{nameof(playerBuilderName)} = {playerBuilderName}");
            stringBuilder.AppendLine($"{nameof(playerHeartbeatTimeout)} = {playerHeartbeatTimeout}");

            if (filters.Length == 0)
            {
                stringBuilder.AppendLine($"{nameof(filters)} = {{}}");
            }

            if (!string.IsNullOrEmpty(customRunnerName))
            {
                stringBuilder.AppendLine($"{nameof(customRunnerName)} = {customRunnerName}");
            }

            for (int i = 0; i < filters.Length; i++)
            {
                stringBuilder.AppendLine($"{nameof(filters)}[{i}] = ");
                var filterStrings = filters[i]
                    .ToString()
                    .Split(new[] {Environment.NewLine}, StringSplitOptions.None)
                    .ToArray();

                foreach (var filterString in filterStrings)
                {
                    stringBuilder.AppendLine($"   {filterString}");
                }
            }

            return stringBuilder.ToString();
        }

        public void OnBeforeSerialize()
        {
            m_TargetPlatform = targetPlatform ?? BuildTarget.NoTarget;
        }

        public void OnAfterDeserialize()
        {
            targetPlatform = m_TargetPlatform == BuildTarget.NoTarget ? null : (BuildTarget?)m_TargetPlatform;
        }

        internal bool PlayerIncluded()
        {
            return targetPlatform != null;
        }

        private static bool IncludesTestMode(TestMode testMode, TestMode modeToCheckFor)
        {
            return (testMode & modeToCheckFor) == modeToCheckFor;
        }

        internal bool CustomRunnerIncluded()
        {
            return !string.IsNullOrEmpty(customRunnerName);
        }
    }
}
