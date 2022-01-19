# How to implement a custom runner
In version 1.2 of the Test Framework package and forward, you can create your own custom runner, which you can then hook into the existing UI and CI setups. This gives you a large amount of flexibility, but it requires your code to handle all of the running of tests itself.

**Note:** You should only create a custom runner as a last resort if the [NUnit extensibility](https://github.com/nunit/docs/wiki/Framework-Extensibility) and [Unity Test Framework extension points](./extending.md) cannot support what you need for your tests. An example of this could be if you want to hook in a different unit testing framework, like XUnit.

## Custom runner implementation
To create a custom runner, create a [CustomRunnerBase](./reference-custom-runner-base.md) implement, which implements the `Execute` and `GetTestList` methods. In the `Execute` implementation call the [Report methods](./reference-custom-runner-base.md#Protected methods), to report test results back to the framework.

To return the test list and report test results, you need to create an implementation of [ITestAdaptor](./reference-itest-adaptor.md) and [ITestResultAdaptor](./reference-itest-result-adaptor.md).

The following is an example of a custom runner, which tests the content of your Scenes. When executing, it looks at all Scenes in the Project and then executes some methods on a provided class. This is by itself a barebone test framework. It registers itself in the static `Load` method, which is run after each domain reload.

``` C#
/// <summary>
/// The SceneTestCustomRunner is an implementation of CustomRunnerBase that allows for running tests on all scenes in a project.
/// This implementation is meant as an example of how the CustomRunner concept can be used.
/// </summary>
public class SceneTestCustomRunner : CustomRunnerBase
{
    // This method is run after each domain reload, registering the CustomRunner with the framework.
    [InitializeOnLoadMethod]
    public static void Load()
    {
        TestRunnerApi.RegisterCustomRunner(new SceneTestCustomRunner());
    }

    // In this example, the tests are made as a list of hardcoded classes, for simplicity.
    private readonly ISceneTest[] tests;
    public SceneTestCustomRunner() : base("Scene Test")
    {
        tests = new ISceneTest[]
        {
            new SceneContainsCameraTest(),
            new SceneContainsLightTest(), 
        };
    }

    // The Execute method is invoked on the runner with information about what filters are currently applied. 
    // The runSynchronously argument indicates whether the run is expected to finish before returning, but since 
    // this implementation is synchronous in any case, then that can be ignored.
    public override void Execute(Filter[] filters, bool runSynchronously)
    {
        try
        {
            var rootSuite = BuildTestTree();
            ReportRunStarted(rootSuite);
            var result = RunTestNode(rootSuite);
            ReportRunFinished(result);
        }
        catch (Exception e)
        {
            ReportRunFailed(e.ToString());
        }
    }

    // The GetTestList implementation builds the tree of tests and provides it to the callback.
    public override void GetTestList(Action<ITestAdaptor> testListCallback)
    {
        var rootSuite = BuildTestTree();
        testListCallback(rootSuite);
    }

    // In this example, the test tree is based on the scenes in the project, creating multiple tests under each scene node.
    private SceneTestAdaptor BuildTestTree()
    {
        var scenes = GetScenePaths();
        var sceneTests = new ITestAdaptor[scenes.Length];

        for (var sceneIndex = 0; sceneIndex < scenes.Length; sceneIndex++)
        {
            var scene = scenes[sceneIndex];
            var testAdaptors = new ITestAdaptor[tests.Length];

            for (int testIndex = 0; testIndex < tests.Length; testIndex++)
            {
                var test = tests[testIndex];
                testAdaptors[testIndex] =
                    new SceneTestAdaptor($"{sceneIndex}:{testIndex}", test.Name, $"{scene}\\{test.Name}", test.RunTest);
            }

            sceneTests[sceneIndex] = new SceneTestAdaptor($"{sceneIndex}", scene, scene,
                () => { EditorSceneManager.OpenScene(scene); }, testAdaptors);
        }

        var rootSuite = new SceneTestAdaptor("root_id", "Scene Tests", "SceneTests", () => {}, sceneTests);
        return rootSuite;
    }

    // The RunTestNode performs the test run on a given node of the tests.
    // If the test is a suite (a non-leaf node), then it calls itself recursively on all children
    // If it is a test (a leaf node), then it runs the test.
    private ITestResultAdaptor RunTestNode(ITestAdaptor test)
    {
        if (test == null)
        {
            throw new ArgumentException("test");
        }
        
        ReportTestStarted(test);
        
        if (test.IsSuite)
        {
            (test as SceneTestAdaptor).Prepare?.Invoke();
            var childResults = test.Children.Select(RunTestNode).ToArray();
            var suiteResult = new SceneTestResultAdaptor(test, childResults);
            ReportTestFinished(suiteResult);
            return suiteResult;
        }

        var resultString = (test as SceneTestAdaptor).RunTest();
        var result = new SceneTestResultAdaptor(test, string.IsNullOrEmpty(resultString) ? TestStatus.Passed : TestStatus.Failed, resultString);
        ReportTestFinished(result);
        return result;
    }

    // The GetScenePaths method queries the asset database to get all paths of scenes in the project.
    private static string[] GetScenePaths()
    {
        var sceneGuids = AssetDatabase.FindAssets("t:Scene");
        var paths = sceneGuids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
        return paths;
    }
}
```
**Note:** The implementation of `ITestAdaptor` and `ITestResultAdaptor` is not included in the above example. The implementations mostly contain the properties defined in the interfaces, in which you can specify the details of your test node and test node result. You should implement the `ToXml` method in [ITestResultAdaptor](./reference-itest-result-adaptor.md), if you want your test run to be able to generate an XML result report.

## Running the tests in the custom runner
If the `CustomRunnerBase` implementation is registered and returning a test tree on `GetTestList`, then it should show up as an additional tab in the test runner window.

![Custom runner in the test runner UI](.../images/custom-runner.png)

You can also trigger the run of the custom runner from the command line, where it is included if the test-platform is set to `custom`.

```.\Unity.exe -projectPath ... -batchmode -runTests -testPlatform custom```



