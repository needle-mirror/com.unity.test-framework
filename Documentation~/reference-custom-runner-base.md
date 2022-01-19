# CustomRunnerBase
The `CustomRunnerBase` allows you to specify a custom implementation to retrieve and run tests. For an overview of the workflow for custom runners, see [How to implement a custom runner](./extension-custom-runner.md).

The `CustomRunnerBase` is an abstract class that can be inherited for you to provide your custom implementation for running tests. 

## Public methods

| Syntax                                     | Description                                                  |
| ------------------------------------------ | ------------------------------------------------------------ |
| `abstract void Execute(Filter[] filters, bool runSynchronously)` | The custom implementation to run tests, which the framework calls when a [Filter](./reference-filter.md) includes the `Custom` testMode. It is up to the custom implementation to go over the filters and determine what, if anything, should be executed. <br><br>If `runSynchronously` is true, the test framework expects the custom runner to finish its run before returning. If this cannot be done, it is a good practice to throw an exception, letting the caller know. |
| `abstract void GetTestList(Action<ITestAdaptor> testListCallback)` | This is the custom implementation to list the tests available to the custom runner. The implementation should construct a tree of [ITestAdaptor](./reference-itest-adaptor.md) and then invoke the provided `testListCallback` callback. |
| `virtual bool PassFilter(Filter filter)` | Checks whether the [Filter](./reference-filter.md) should result in the custom runner being executed. All filters in the run are tested against this method and if any of them matches, then the framework calls `Execute`, with the matching filters. <br><br>The default implementation checks if the filter has the `Custom` testMode. Override this implementation if you want to control whether your `Execute` is invoked. |

## Protected methods
The implementation `CustomRunnerBase` contains a set of protected methods that your implementation can use to report back results of the run. These are expected to be invoked in a specific order (see [ICallbacks](./reference-icallbacks.md) for details).

| Syntax                                     | Description                                                  |
| ------------------------------------------ | ------------------------------------------------------------ |
| `void ReportRunStarted(ITestAdaptor testsToRun)` | Report to the framework that the run on a given test tree has started. |
| `void ReportTestStarted(ITestAdaptor test)` | Report to the framework that a node of the test tree has started running. |
| `void ReportTestFinished(ITestResultAdaptor result)` | Report that a node of the test tree has finished running with a given result. |
| `void ReportRunFinished(ITestResultAdaptor testResults)` | Report that the full test run has finished with a given result. |
| `void ReportRunFailed(string failureMessage)` | Report that a test run has failed to run with a given message. |
