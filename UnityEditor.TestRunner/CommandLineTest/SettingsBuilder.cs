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
        private Func<string, bool> m_FileExistsCheck;
        private Func<bool> m_ScriptCompilationFailedCheck;
        public SettingsBuilder(ITestSettingsDeserializer testSettingsDeserializer, Action<string> logAction, Action<string> logWarningAction, Func<string, bool> fileExistsCheck, Func<bool> scriptCompilationFailedCheck)
        {
            m_LogAction = logAction;
            m_LogWarningAction = logWarningAction;
            m_FileExistsCheck = fileExistsCheck;
            m_ScriptCompilationFailedCheck = scriptCompilationFailedCheck;
            m_TestSettingsDeserializer = testSettingsDeserializer;
        }

        public Api.ExecutionSettings BuildApiExecutionSettings(string[] commandLineArgs)
        {
            var quit = false;
            string testPlatform = string.Empty;
            string[] testFilters = null;
            string[] testCategories = null;
            string testSettingsFilePath = null;
            int testRepetitions = 1;
            int? playerHeartbeatTimeout = null;
            bool runSynchronously = false;
            string[] testAssemblyNames = null;
            string playerBuilderName = null;
            bool? requiresPlayMode = null;
            string assemblyType = null;
            string customRunnerName = string.Empty;
            string[] testNames = null;

            var optionSet = new CommandLineOptionSet(
                new CommandLineOption("quit", () => { quit = true; }),
                new CommandLineOption("testPlatform", platform => { testPlatform = platform; }),
                new CommandLineOption("requiresPlayMode", requiresPlayModeString =>
                {
                    bool reqPlayMode;
                    if (Boolean.TryParse(requiresPlayModeString, out reqPlayMode))
                    {
                        requiresPlayMode = reqPlayMode;
                    }
                }),
                new CommandLineOption("assemblyType", type => { assemblyType = type; }),
                new CommandLineOption("editorTestsFilter", filters => { testFilters = filters; }),
                new CommandLineOption("testFilter", filters => { testFilters = filters; }),
                new CommandLineOption("editorTestsCategories", categories => { testCategories = categories; }),
                new CommandLineOption("testCategory", categories => { testCategories = categories; }),
                new CommandLineOption("testSettingsFile", settingsFilePath => { testSettingsFilePath = settingsFilePath; }),
                new CommandLineOption("testRepetitions", reps => { testRepetitions = int.Parse(reps); }),
                new CommandLineOption("playerHeartbeatTimeout", timeout => { playerHeartbeatTimeout = int.Parse(timeout); }),
                new CommandLineOption("runSynchronously", () => { runSynchronously = true; }),
                new CommandLineOption("assemblyNames", assemblyNames => { testAssemblyNames = assemblyNames.Split(';'); }),
                new CommandLineOption("playerBuilderName", builderName => { playerBuilderName = builderName; }),
                new CommandLineOption("customRunner", customRunner => { customRunnerName = customRunner; }),
                new CommandLineOption("testNames", name => { testNames = name.Split(';'); })
            );
            optionSet.Parse(commandLineArgs);

            DisplayQuitWarningIfQuitIsGiven(quit);

            CheckForScriptCompilationErrors();

            ValidateCustomRunner(customRunnerName);

            var testSettings = GetTestSettings(testSettingsFilePath);
            var filter = new Filter()
            {
                groupNames = testFilters,
                categoryNames = testCategories,
                assemblyNames = testAssemblyNames,
                requiresPlayMode = requiresPlayMode,
                testNames = testNames
            };
            filter = ModifyApiFilter(filter, testPlatform, assemblyType, customRunnerName);

            var settings = new Api.ExecutionSettings()
            {
                filters = new[] {filter},
                overloadTestRunSettings = new RunSettings(testSettings),
                targetPlatform = GetBuildTarget(testPlatform),
                runSynchronously = runSynchronously,
                playerBuilderName = playerBuilderName,
                customRunnerName = customRunnerName
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

            return new ExecutionSettings()
            {
                TestResultsFile = resultFilePath,
                DeviceLogsDirectory = deviceLogsDirectory
            };
        }

        void DisplayQuitWarningIfQuitIsGiven(bool quitIsGiven)
        {
            if (quitIsGiven)
            {
                m_LogWarningAction("Running tests from command line arguments will not work when \"quit\" is specified.");
            }
        }

        void CheckForScriptCompilationErrors()
        {
            if (m_ScriptCompilationFailedCheck())
            {
                throw new SetupException(SetupException.ExceptionType.ScriptCompilationFailed);
            }
        }

        ITestSettings GetTestSettings(string testSettingsFilePath)
        {
            ITestSettings testSettings = null;
            if (!string.IsNullOrEmpty(testSettingsFilePath))
            {
                if (!m_FileExistsCheck(testSettingsFilePath))
                {
                    throw new SetupException(SetupException.ExceptionType.TestSettingsFileNotFound, testSettingsFilePath);
                }

                testSettings = m_TestSettingsDeserializer.GetSettingsFromJsonFile(testSettingsFilePath);
            }
            return testSettings;
        }

        private static void ValidateCustomRunner(string customRunner)
        {
            if (string.IsNullOrEmpty(customRunner))
            {
                return;
            }

            if (TestRunnerApi.GetCustomRunner(customRunner) != null)
            {
                return;
            }

            throw new SetupException(SetupException.ExceptionType.CustomRunnerNotFound, customRunner);
        }

        static Filter ModifyApiFilter(Filter filter, string testPlatform, string assemblyType, string customRunner)
        {
            if (testPlatform.ToLower() == "editmode")
            {
                filter.assemblyType = AssemblyType.EditorOnly;
            }
            else if (testPlatform.ToLower() == "playmode")
            {
                filter.assemblyType = AssemblyType.EditorAndPlatforms;
            }
            if (!string.IsNullOrEmpty(assemblyType))
            {
                filter.assemblyType = (AssemblyType)Enum.Parse(typeof(AssemblyType), assemblyType, true);
            }

            return filter;
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
