using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Internal.Filters;
using TestRun.Tasks.Player;
using UnityEditor.TestTools.TestRunner.TestRun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;

namespace UnityEditor.TestTools.TestRunner.Api
{
    /// <summary>
    /// The TestRunnerApi retrieves and runs tests programmatically from code inside the project, or inside other packages. TestRunnerApi is a [ScriptableObject](https://docs.unity3d.com/ScriptReference/ScriptableObject.html).
    /// You can initialize the API like this:
    /// <code>
    /// var testRunnerApi = ScriptableObject.CreateInstance&lt;TestRunnerApi&gt;();
    /// </code>
    /// You can subscribe and receive test results in one instance of the API, even if the run starts from another instance.
    /// The TestRunnerApi supports the following workflows:
    /// - [Running tests programmatically](#run-tests)
    /// - [Gettting test results](#get-test-results)
    /// - [Retrieving the list of tests](#retrieve-list-of-tests)
    ///
    /// > **Note:** Non-static methods in this class will become obsolete in future versions so you should use the static methods (e.g. RegisterTestCallback, ExecuteTestRun) rather than their non-static equivalents (e.g. RegisterCallbacks, Execute).
    /// </summary>
    /// <example>
    /// ### Run tests
    /// Run tests by calling `ExecuteTestRun` and providing some execution settings in the form of a <see cref="Filter"/>. The `Filter` specifies what tests to run.
    /// The following is an example of how to run all **Play Mode** tests in a project:
    /// <code>
    /// <![CDATA[
    /// var filter = new Filter()
    /// {
    ///     testMode = TestMode.PlayMode
    /// };
    ///     TestRunnerApi.ExecuteTestRun(new ExecutionSettings(filter));
    /// ]]>
    /// </code>
    /// #### Multiple filter values
    /// You can specify a more specific filter by filling out the fields on the `Filter` class in more detail. Many of the fields allow for multiple values. The runner runs any test that matches at least one of the specified values.
    /// In this example, the API runs tests with full names that match either of the two names provided:
    /// <code>
    /// <![CDATA[
    /// TestRunnerApi.ExecuteTestRun(new ExecutionSettings(new Filter()
    /// {
    ///     testNames = new[] {"MyTestClass.NameOfMyTest", "SpecificTestFixture.NameOfAnotherTest"}
    /// }));
    /// ]]>
    /// </code>
    /// #### Multiple filter fields
    /// If using multiple different fields on the filter, then the runner runs tests that match each of the different fields.
    /// In this example, a test is run if it matches either of the two test names **and** belongs to an assembly with the specified name:
    /// <code>
    /// <![CDATA[
    /// TestRunnerApi.ExecuteTestRun(new ExecutionSettings(new Filter()
    /// {
    ///     assemblyNames = new [] {"MyTestAssembly"},
    ///     testNames = new [] {"MyTestClass.NameOfMyTest", "MyTestClass.AnotherNameOfATest"}
    /// }));
    /// ]]>
    /// </code>
    /// #### Multiple constructor filters
    /// <see cref="ExecutionSettings"/> takes one or more filters in its constructor. If there is no filter provided, then it runs all **Edit Mode** tests by default. If there are multiple filters provided, then a test runs if it matches any of the filters.
    /// In this example, it runs any tests in the assembly named `MyTestAssembly` and any test with a full name matching either of the two provided test names:
    /// <code>
    /// <![CDATA[
    /// TestRunnerApi.ExecuteTestRun(new ExecutionSettings(
    ///     new Filter()
    ///     {
    ///         assemblyNames = new[] { "MyTestAssembly" },
    ///     },
    ///     new Filter()
    ///     {
    ///         testNames = new[] { "MyTestClass.NameOfMyTest", "MyTestClass.AnotherNameOfATest" }
    ///     }
    /// ));
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    /// ### Get test results
    /// You can receive callbacks when the active test run, or an individual test, starts and finishes. You can register callbacks by invoking `RegisterTestCallback` with an instance of a class that implements <see cref="ICallbacks"/>. There are four `ICallbacks` methods for the start and finish of both the whole run and each level of the test tree. An example of how listeners can be set up:
    /// > **Note**: Listeners receive callbacks from all test runs, regardless of the registered `TestRunnerApi` for that instance.
    ///
    /// <code>
    /// <![CDATA[
    /// public void SetupListeners()
    /// {
    ///     TestRunnerApi.RegisterTestCallback(new MyCallbacks());
    /// }
    ///
    /// private class MyCallbacks : ICallbacks
    /// {
    ///     public void RunStarted(ITestAdaptor testsToRun)
    ///     {
    ///
    ///     }
    ///
    ///     public void RunFinished(ITestResultAdaptor result)
    ///     {
    ///
    ///     }
    ///
    ///     public void TestStarted(ITestAdaptor test)
    ///     {
    ///
    ///     }
    ///
    ///     public void TestFinished(ITestResultAdaptor result)
    ///     {
    ///        if (!result.HasChildren && result.ResultState != "Passed")
    ///        {
    ///            Debug.Log(string.Format("Test {0} {1}", result.Test.Name, result.ResultState));
    ///        }
    ///     }
    /// }
    ///
    /// ]]>
    /// </code>
    /// > **Note**: The registered callbacks are not persisted on domain reloads. So it is necessary to re-register the callback after a domain reload, usually with [InitializeOnLoad](https://docs.unity3d.com/Manual/RunningEditorCodeOnLaunch.html).
    ///
    /// It's possible to provide a `priority` as an integer as the second argument when registering a callback. This influences the invocation order of different callbacks. The default value is zero. It's also possible to provide `RegisterTestCallback` with a class instance that implements <see cref="IErrorCallbacks"/> that is an extended version of `ICallbacks`. `IErrorCallbacks` also has a callback method for `OnError` that invokes if the run fails to start, for example, due to compilation errors or if an <see cref="IPrebuildSetup"/> throws an exception.
    /// </example>
    /// <example>
    /// ### Retrieve list of tests
    /// You can retrieve the test tree by invoking `RetrieveTestTree` with the desired <see cref="ExecutionSettings"/> and a callback action, with an <see cref="ITestAdaptor"/> representing the test tree.
    /// <code>
    /// <![CDATA[
    /// var filter = new Filter()
    /// {
    ///     assemblyNames = new [] {"myTestAssembly"}
    /// };
    /// TestRunnerApi.RetrieveTestTree(new ExecutionSettings(filter), (testRoot) =>
    /// {
    ///     Debug.Log(string.Format("Tree contains {0} tests.", testRoot.TestCaseCount));
    /// });
    /// ]]>
    /// </code>
    /// </example>
    public class TestRunnerApi : ScriptableObject, ITestRunnerApi
    {
        internal static ICallbacksHolder callbacksHolder;
        private static Dictionary<string, IPlayerBuilder> m_PlayerBuilders;

        private static ICallbacksHolder CallbacksHolder
        {
            get
            {
                if (callbacksHolder == null)
                {
                    callbacksHolder = Api.CallbacksHolder.instance;
                }

                return callbacksHolder;
            }
        }

        internal static ITestJobDataHolder testJobDataHolder;

        private static ITestJobDataHolder m_testJobDataHolder
        {
            get { return testJobDataHolder ?? (testJobDataHolder = TestJobDataHolder.instance); }
        }


        internal static ICustomRunnerHolder customRunnerHolder;

        private static ICustomRunnerHolder m_customRunnerHolder
        {
            get
            {
                if (customRunnerHolder == null)
                {
                    customRunnerHolder = CustomRunnerHolder.instance;
                }

                return customRunnerHolder;
            }
        }

        internal static Func<ExecutionSettings, string> ScheduleJob = (executionSettings) =>
        {
            var runner = new TestJobRunner();
            var jobData = new TestJobData(executionSettings);
            runner.RunJob(jobData);
            return jobData.guid;
        };
        /// <summary>
        /// Starts a test run with a given set of executionSettings.
        /// </summary>
        /// <param name="executionSettings">Set of <see cref="ExecutionSettings"/></param>
        /// <returns>A GUID that identifies the TestJobData.</returns>
        public string Execute(ExecutionSettings executionSettings)
        {
            return ExecuteTestRun(executionSettings);
        }

        /// <summary>
        /// Starts a test run with a given set of executionSettings.
        /// </summary>
        /// <param name="executionSettings">Set of <see cref="ExecutionSettings"/></param>
        /// <returns>A GUID that identifies the TestJobData.</returns>
        public static string ExecuteTestRun(ExecutionSettings executionSettings)
        {
            if (executionSettings == null)
            {
                throw new ArgumentNullException(nameof(executionSettings));
            }

            if ((executionSettings.filters == null || executionSettings.filters.Length == 0) && executionSettings.filter != null)
            {
                // Map filter (singular) to filters (plural), for backwards compatibility.
                executionSettings.filters = new[] {executionSettings.filter};
            }

            if (executionSettings.targetPlatform == null && executionSettings.filters != null &&
                executionSettings.filters.Length > 0)
            {
                executionSettings.targetPlatform = executionSettings.filters[0].targetPlatform;
            }

            if (executionSettings.filters != null && executionSettings.filters.Length > 0)
            {
                executionSettings.filters[0].targetPlatform = executionSettings.targetPlatform;
            }

            return ScheduleJob(executionSettings);
        }

        /// <summary>
        /// Sets up a given instance of <see cref="ICallbacks"/> to be invoked on test runs.
        /// </summary>
        /// <typeparam name="T">
        /// Generic representing a type of callback.
        /// </typeparam>
        /// <param name="testCallbacks">
        /// The test callbacks to be invoked.
        /// </param>
        /// <param name="priority">
        /// Sets the order in which the callbacks are invoked, starting with the highest value first.
        /// </param>
        public void RegisterCallbacks<T>(T testCallbacks, int priority = 0) where T : ICallbacks
        {
            RegisterTestCallback(testCallbacks);
        }

        /// <summary>
        /// Sets up a given instance of <see cref="ICallbacks"/> to be invoked on test runs.
        /// </summary>
        /// <typeparam name="T">
        /// Generic representing a type of callback.
        /// </typeparam>
        /// <param name="testCallbacks">The test callbacks to be invoked</param>
        /// <param name="priority">
        /// Sets the order in which the callbacks are invoked, starting with the highest value first.
        /// </param>
        public static void RegisterTestCallback<T>(T testCallbacks, int priority = 0) where T : ICallbacks
        {
            if (testCallbacks == null)
            {
                throw new ArgumentNullException(nameof(testCallbacks));
            }

            CallbacksHolder.Add(testCallbacks, priority);
        }

        /// <summary>
        /// Unregister an instance of <see cref="ICallbacks"/> to no longer receive callbacks from test runs.
        /// </summary>
        /// <typeparam name="T">
        /// Generic representing a type of callback.
        /// </typeparam>
        /// <param name="testCallbacks">The test callbacks to unregister.</param>
        public void UnregisterCallbacks<T>(T testCallbacks) where T : ICallbacks
        {
            UnregisterTestCallback(testCallbacks);
        }

        /// <summary>
        /// Unregister an instance of <see cref="ICallbacks"/> to no longer receive callbacks from test runs.
        /// </summary>
        /// <typeparam name="T">
        /// Generic representing a type of callback.
        /// </typeparam>
        /// <param name="testCallbacks">The test callbacks to unregister.</param>
        public static void UnregisterTestCallback<T>(T testCallbacks) where T : ICallbacks
        {
            if (testCallbacks == null)
            {
                throw new ArgumentNullException(nameof(testCallbacks));
            }

            CallbacksHolder.Remove(testCallbacks);
        }

        /// <summary>
        /// Registers an implementation of <see cref="CustomRunnerBase"/> with the test framework, allowing for running custom test frameworks.
        /// Since custom runners are identified by name, the provided custom runner must use a name that has not already been registered.
        /// </summary>
        /// <param name="customRunner">The custom runner to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="customRunner"/> argument is null.</exception>
        public static void RegisterCustomRunner(CustomRunnerBase customRunner)
        {
            if (customRunner == null)
            {
                throw new ArgumentNullException(nameof(customRunner));
            }

            var existingRunner = GetCustomRunner(customRunner.name);
            if (existingRunner == customRunner)
            {
                Debug.LogWarning($"The custom runner '{customRunner.name}' has already been registered.");
                return;
            }

            if (existingRunner != null)
            {
                Debug.LogWarning($"A custom runner named '{existingRunner.name}' has already been registered. Custom runner names must be unique.");
                return;
            }

            m_customRunnerHolder.Add(customRunner);
        }

        /// <summary>
        /// Unregisters a custom runner from the test framework.
        /// </summary>
        /// <param name="customRunner">The custom runner to unregister.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="customRunner"/> argument is null.</exception>
        public static void UnregisterCustomRunner(CustomRunnerBase customRunner)
        {
            if (customRunner == null)
            {
                throw new ArgumentNullException(nameof(customRunner));
            }

            if (GetCustomRunner(customRunner.name) != customRunner)
            {
                Debug.LogWarning($"Attempting to unregister an unknown custom runner '{customRunner.name}'. The request will be ignored.");
                return;
            }

            m_customRunnerHolder.Remove(customRunner);
        }

        /// <summary>
        /// Get the custom runner associated with the provided name. The search is case agnostic.
        /// </summary>
        /// <param name="customRunnerName">The name of the custom runner being requested.</param>
        /// <returns>The custom runner instance or null if no custom runner with the provided name has been registered.</returns>
        internal static CustomRunnerBase GetCustomRunner(string customRunnerName)
        {
            return m_customRunnerHolder.Get(customRunnerName);
        }

        /// <summary>
        /// Get all the names of the currently registered custom runners.
        /// </summary>
        /// <returns>A list of custom runner names.</returns>
        public static string[] GetCustomRunnerNames()
        {
            return m_customRunnerHolder.GetAll().Select(customRunner => customRunner.name).ToArray();
        }

        /// <summary>
        /// Retrieve a list of tests in a tree structure matching a given execution settings.
        /// </summary>
        /// <param name="executionSettings">An execution setting to match. The filters in the settings will be checked against the test tree.</param>
        /// <param name="callback">A callback to invoke with the result of the test retrieving.</param>
        /// <exception cref="ArgumentNullException">An exception is thrown if the executionSettings or the callback arguments are null.</exception>
        public static void RetrieveTestTree(ExecutionSettings executionSettings, Action<ITestAdaptor> callback)
        {
            if (executionSettings == null)
            {
                throw new ArgumentNullException(nameof(executionSettings));
            }

            if (executionSettings.filters == null)
            {
                throw new ArgumentNullException(nameof(executionSettings.filters));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var customRunnerName = executionSettings.customRunnerName;
            if (!string.IsNullOrEmpty(customRunnerName))
            {
                var customRunner = GetCustomRunner(customRunnerName);
                if (customRunner == null)
                {
                    throw new ArgumentException($"The selected custom runner '{customRunnerName}' was not found.");
                }

                customRunner.GetTestList(callback);
                return;
            }

            var testAssemblyProvider = new EditorLoadedTestAssemblyProvider(new EditorCompilationInterfaceProxy(), new EditorAssembliesProxy());
            var testListProvider = new TestListProvider(testAssemblyProvider, new UnityTestAssemblyBuilder());
            var cachedTestListProvider = new CachingTestListProvider(testListProvider, new TestListCache(),  new TestAdaptorFactory());

            var filter = new OrFilter(executionSettings.filters.Select(f => f.ToRuntimeTestRunnerFilter(executionSettings.runSynchronously).BuildNUnitFilter()).ToArray());

            var job = new TestListJob(cachedTestListProvider, filter, (testRoot) =>
            {
                callback(testRoot);
            });
            job.Start();
        }

        /// <summary>
        /// Retrieve the full test tree as ITestAdaptor for a given test mode.
        /// </summary>
        /// <param name="testMode"></param>
        /// <param name="callback"></param>
        public void RetrieveTestList(TestMode testMode, Action<ITestAdaptor> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            Filter filter = null;

            // Translate the old testMode notion into assembly type
            switch (testMode)
            {
                case TestMode.EditMode:
                    filter = new Filter() {assemblyType = AssemblyType.EditorOnly};
                    break;
                case TestMode.PlayMode:
                    filter = new Filter() {assemblyType = AssemblyType.EditorAndPlatforms};
                    break;
                case TestMode.EditMode | TestMode.PlayMode:
                    filter = new Filter() {assemblyType = AssemblyType.EditorAndPlatforms | AssemblyType.EditorOnly};
                    break;
            }

            RetrieveTestTree(new ExecutionSettings(filter), callback);
        }

        ///<summary>
        /// Save a given set of ITestResultAdaptor to a file at the provided file path.
        /// </summary>
        /// <param name="results">Test results to write in a file.</param>
        /// <param name="filePath">Path to file.</param>
        public static void SaveResultToFile(ITestResultAdaptor results, string filePath)
        {
            var resultsWriter = new ResultsWriter();
            resultsWriter.WriteResultToFile(results, filePath);
        }

        /// <summary>
        /// Cancel the test run with a given guid. The guid can be retrieved when executing the test run. Currently only supports EditMode tests.
        /// </summary>
        /// <param name="guid">Test run GUID.</param>
        /// <returns></returns>
        public static bool CancelTestRun(string guid)
        {
            var runner = m_testJobDataHolder.GetRunner(guid);
            if (runner == null || !runner.IsRunningJob())
            {
                return false;
            }

            return runner.CancelRun();
        }

        /// <summary>
        /// Cancels all running test runs. Currently only supports EditMode tests.
        /// </summary>
        public static void CancelAllTestRuns()
        {
            foreach (var runner in m_testJobDataHolder.GetAllRunners())
            {
                if (runner != null && runner.IsRunningJob())
                {
                    runner.CancelRun();
                }
            }
        }

        /// <summary>
        /// Checks if any current test run is currently running.
        /// </summary>
        /// <returns>A boolean indicating if any test run is currently running.</returns>
        public static bool IsAnyRunActive()
        {
            return m_testJobDataHolder.GetAllRunners().Any(runner => runner.IsRunningJob());
        }

        /// <summary>
        /// Checks if a test run with a given guid is active.
        /// </summary>
        /// <param name="guid">The guid of the test run to check for.</param>
        /// <returns>A boolean indicating if the test run is currently running.</returns>
        /// <exception cref="Exception">Throws an exception if no run data for the guid could be found.</exception>
        public static bool IsRunActive(string guid)
        {
            var runner = m_testJobDataHolder.GetRunner(guid);
            if (runner == null)
            {
                throw new RunnerNotFoundException(guid);
            }

            return runner.IsRunningJob();
        }

        /// <summary>
        /// Provides a list of guids for all test runs that are currently active.
        /// </summary>
        /// <returns>A list of guids of active runs.</returns>
        public static string[] GetActiveRunGuids()
        {
            return m_testJobDataHolder.GetAllRunners()
                .Where(runner => runner.IsRunningJob())
                .Select(runner => runner.GetData()?.guid)
                .Where(guid => !string.IsNullOrEmpty(guid))
                .ToArray();
        }

        /// <summary>
        /// Gets the execution settings for a test run with a specific guid.
        /// </summary>
        /// <param name="guid">The guid of the test run.</param>
        /// <returns>The execution settings for the run.</returns>
        /// <exception cref="Exception">Throws an exception if no run data for the guid could be found.</exception>
        public static ExecutionSettings GetExecutionSettings(string guid)
        {
            var runner = m_testJobDataHolder.GetRunner(guid);
            if (runner == null)
            {
                throw new Exception($"Could not find runner with id {guid}.");
            }

            return runner.GetData()?.executionSettings;
        }

        internal static DateTime GetRunStartTime(string guid)
        {
            var runner = m_testJobDataHolder.GetRunner(guid);
            if (runner == null)
            {
                throw new Exception($"Could not find runner with id {guid}.");
            }

            return Convert.ToDateTime(runner.GetData().startTime);
        }

        internal class RunProgressChangedEvent : UnityEvent<TestRunProgress> {}
        internal static RunProgressChangedEvent runProgressChanged = new RunProgressChangedEvent();

        /// <summary>
        /// Collects all player builders from assembly domain.
        /// Note: Previously the builders themselves were calling RegisterPlayerBuilder using InitializeOnLoadMethod attribute
        /// But it appeared the registering happens too late, meaning after TestStarter:Initialize call, thus the player builders wouldn't be found in some cases
        /// </summary>
        private static void CollectPlayerBuildersIfNeeded()
        {
            if (m_PlayerBuilders != null)
            {
                return;
            }

            m_PlayerBuilders = new Dictionary<string, IPlayerBuilder>();

            var types = TypeCache.GetTypesDerivedFrom<IPlayerBuilder>();
            foreach (var type in types)
            {
                IPlayerBuilder builder;
                try
                {
                    builder = (IPlayerBuilder)Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, $"Failed to instantiate PlayerBuilder of type {type.Name}.");
                    Debug.LogException(e);
                    continue;
                }

                if (string.IsNullOrEmpty(builder.Name))
                {
                    Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, $"Player builder of type {builder.GetType().Name} had a null or empty name.");
                }
                else if (m_PlayerBuilders.ContainsKey(builder.Name))
                {
                    Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, $"Player builder with name {builder.Name} was already added");
                }
                else
                {
                    m_PlayerBuilders[builder.Name] = builder;
                }
            }

            if (m_PlayerBuilders.Count == 0)
            {
                Debug.LogError($"No valid player builders found. The default player builder should always be present. Found {types.Count} matching implementations.");
            }
        }

        internal static IPlayerBuilder GetPlayerBuilderFromName(string name)
        {
            CollectPlayerBuildersIfNeeded();
            if (string.IsNullOrEmpty(name))
            {
                return m_PlayerBuilders[GetDefaultPlayerBuilderName()];
            }

            if (m_PlayerBuilders.TryGetValue(name, out var value))
            {
                return value;
            }

            Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, $"Failed to find player builder '{name}', using the default player builder instead.");
            return m_PlayerBuilders[GetDefaultPlayerBuilderName()];
        }

        internal static string[] GetPlayerBuilderNames()
        {
            CollectPlayerBuildersIfNeeded();
            if (m_PlayerBuilders == null || m_PlayerBuilders.Count == 0)
            {
                return new string[0];
            }

            return m_PlayerBuilders.Keys.ToArray();
        }

        internal static string GetDefaultPlayerBuilderName()
        {
            return DefaultPlayerBuilder.k_Name;
        }
    }
}
