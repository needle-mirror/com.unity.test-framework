using System;
using System.IO;
using System.Linq;
using UnityEditor.TestRunner.CommandLineParser;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.GUI;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.CommandLineTest
{
    internal class SettingsBuilder : ISettingsBuilder
    {
        private ITestSettingsDeserializer m_TestSettingsDeserializer;
        private Action<string> m_LogAction;
        private Action<string> m_LogWarningAction;
        internal Func<string, bool> fileExistsCheck = File.Exists;
        private Func<bool> m_ScriptCompilationFailedCheck;
        internal Func<string, string[]> readAllLines = filePath => File.ReadAllText(filePath).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        public SettingsBuilder(ITestSettingsDeserializer testSettingsDeserializer, Action<string> logAction, Action<string> logWarningAction, Func<bool> scriptCompilationFailedCheck)
        {
            m_LogAction = logAction;
            m_LogWarningAction = logWarningAction;
            m_ScriptCompilationFailedCheck = scriptCompilationFailedCheck;
            m_TestSettingsDeserializer = testSettingsDeserializer;
        }

        public Api.ExecutionSettings BuildApiExecutionSettings(string[] commandLineArgs)
        {
            var quit = false;
            string testPlatform = TestMode.EditMode.ToString();
            string[] testFilters = null;
            string[] testCategories = null;
            string testSettingsFilePath = null;
            int testRepetitions = 1;
            int? playerHeartbeatTimeout = null;
            bool runSynchronously = false;
            string[] testAssemblyNames = null;
            string buildPlayerPath = string.Empty;
            string orderedTestListFilePath = null;


            var optionSet = new CommandLineOptionSet(
                new CommandLineOption("quit", () => { quit = true; }),
                new CommandLineOption("testPlatform", platform => { testPlatform = platform; }),
                new CommandLineOption("editorTestsFilter", filters => { testFilters = filters; }),
                new CommandLineOption("testFilter", filters => { testFilters = filters; }),
                new CommandLineOption("editorTestsCategories", catagories => { testCategories = catagories; }),
                new CommandLineOption("testCategory", catagories => { testCategories = catagories; }),
                new CommandLineOption("testSettingsFile", settingsFilePath => { testSettingsFilePath = settingsFilePath; }),
                new CommandLineOption("testRepetitions", reps => { testRepetitions = int.Parse(reps); }),
                new CommandLineOption("playerHeartbeatTimeout", timeout => { playerHeartbeatTimeout = int.Parse(timeout); }),
                new CommandLineOption("runSynchronously", () => { runSynchronously = true; }),
                new CommandLineOption("assemblyNames", assemblyNames => { testAssemblyNames = assemblyNames; }),
                new CommandLineOption("buildPlayerPath", buildPath => { buildPlayerPath = buildPath; }),
                new CommandLineOption("orderedTestListFile", filePath => { orderedTestListFilePath = filePath; })
            );
            optionSet.Parse(commandLineArgs);

            DisplayQuitWarningIfQuitIsGiven(quit);

            CheckForScriptCompilationErrors();

            var testSettings = GetTestSettings(testSettingsFilePath);
            var filter = new Filter
            {
                testMode = testPlatform.ToLower() == "editmode" ? TestMode.EditMode : TestMode.PlayMode,
                groupNames = testFilters,
                categoryNames = testCategories,
                assemblyNames = testAssemblyNames
            };

            var settings = new Api.ExecutionSettings
            {
                filters = new []{ filter },
                overloadTestRunSettings = new RunSettings(testSettings),
                targetPlatform = GetBuildTarget(testPlatform),
                runSynchronously = runSynchronously,
                playerSavePath = buildPlayerPath,
                orderedTestNames = GetOrderedTestList(orderedTestListFilePath)
            };

            if (playerHeartbeatTimeout != null)
            {
                settings.playerHeartbeatTimeout = playerHeartbeatTimeout.Value;
            }

            return settings;
        }

        public ExecutionSettings BuildExecutionSettings(string[] commandLineArgs)
        {
            string resultFilePath = null;
            string deviceLogsDirectory = null;

            var optionSet = new CommandLineOptionSet(
                new CommandLineOption("editorTestsResultFile", filePath => { resultFilePath = filePath; }),
                new CommandLineOption("testResults", filePath => { resultFilePath = filePath; }),
                new CommandLineOption("deviceLogs", dirPath => { deviceLogsDirectory = dirPath; })
            );
            optionSet.Parse(commandLineArgs);

            return new ExecutionSettings
            {
                TestResultsFile = resultFilePath,
                DeviceLogsDirectory = deviceLogsDirectory
            };
        }

        private void DisplayQuitWarningIfQuitIsGiven(bool quitIsGiven)
        {
            if (quitIsGiven)
            {
                m_LogWarningAction("Running tests from command line arguments will not work when \"quit\" is specified.");
            }
        }

        private void CheckForScriptCompilationErrors()
        {
            if (m_ScriptCompilationFailedCheck())
            {
                throw new SetupException(SetupException.ExceptionType.ScriptCompilationFailed);
            }
        }

        private ITestSettings GetTestSettings(string testSettingsFilePath)
        {
            ITestSettings testSettings = null;
            if (!string.IsNullOrEmpty(testSettingsFilePath))
            {
                if (!fileExistsCheck(testSettingsFilePath))
                {
                    throw new SetupException(SetupException.ExceptionType.TestSettingsFileNotFound, testSettingsFilePath);
                }

                testSettings = m_TestSettingsDeserializer.GetSettingsFromJsonFile(testSettingsFilePath);
            }
            return testSettings;
        }
        
        private string[] GetOrderedTestList(string orderedTestListFilePath)
        {
            if (!string.IsNullOrEmpty(orderedTestListFilePath))
            {
                if (!fileExistsCheck(orderedTestListFilePath))
                {
                    throw new SetupException(SetupException.ExceptionType.OrderedTestListFileNotFound, orderedTestListFilePath);
                }

                return readAllLines(orderedTestListFilePath);
            }
            return null;
        }

        private static BuildTarget? GetBuildTarget(string testPlatform)
        {
            var testPlatformLower = testPlatform.ToLower();
            if (testPlatformLower == "editmode" || testPlatformLower == "playmode" || testPlatformLower == "editor" ||
                string.IsNullOrEmpty(testPlatformLower))
            {
                return null;
            }

            try
            {
                return (BuildTarget)Enum.Parse(typeof(BuildTarget), testPlatform, true);
            }
            catch (ArgumentException)
            {
                throw new SetupException(SetupException.ExceptionType.PlatformNotFound, testPlatform);
            }
        }
    }
}
