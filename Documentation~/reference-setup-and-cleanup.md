# Setup and cleanup at build time

Sometimes you want to make changes to Unity or to the file system before building tests, and to clean up such changes after the test run. You can do pre-build setup and post-build cleanup in your tests in one of the following ways: 

1. Implement [IPrebuildSetup](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEngine.TestTools.IPrebuildSetup.html) and/or [IPostBuildCleanup](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEngine.TestTools.IPostBuildCleanup.html) interfaces in your test class.
2. Apply the [PrebuildSetup](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEngine.TestTools.PrebuildSetupAttribute.html) attribute and [PostBuildCleanup](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEngine.TestTools.PostBuildCleanupAttribute.html) attribute to your test class, to one of the tests, or to the test assembly, providing a class name that implements the corresponding interface as an argument (e.g. `[PrebuildSetup("MyTestSceneSetup")]`). 

## Execution order

All setups run in a deterministic order one after another. The first to run are the setups defined with attributes. Then any test class implementing the interface runs, in alphabetical order inside their namespace, which is the same order tests run in.

> **Note**: Cleanup runs right away for a standalone test run, but only after related tests run in the Unity Editor.

## PrebuildSetup and PostBuildCleanup

Both `PrebuildSetup` and `PostBuildCleanup` attributes run if the respective test or test class is in the current test run. The test is included either by running all tests or setting a [filter](./workflow-create-test.md#filters) that includes the test. If multiple tests reference the same pre-build setup or post-build cleanup, then it only runs once.

