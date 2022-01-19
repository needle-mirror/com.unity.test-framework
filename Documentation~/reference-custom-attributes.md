# Custom attributes

As a part of Unity Test Framework's public API we provide the following attributes:

## Attributes

| Attribute                  | Description                                                  |
| -------------------------- | ------------------------------------------------------------ |
| `ParameterizedIgnore`       | A custom alternative to NUnit `Ignore` that allows ignoring tests based on parameters passed to the test method. See [ParameterizedIgnoreAttribute](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEngine.TestTools.ParameterizedIgnoreAttribute.html).
| `PostBuildCleanup`         | Make changes to Unity or the file system after building. See [Setup and cleanup at build time](./reference-setup-and-cleanup.md).
| `PrebuildSetup`            | Make changes to Unity or the file system before building. See [Setup and cleanup at build time](./reference-setup-and-cleanup.md).
| `PreservedValues`          | Like NUnit `Values` this is used to provide literal arguments for an individual test parameter. See [PreservedValuesAttribute](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEngine.TestTools.PreservedValuesAttribute.html).
| `RequirePlatformSupport`   | Require Player build support for the specified platforms in order to run tests. See [RequirePlayformSupportAttribute](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEditor.TestTools.RequirePlatformSupportAttribute.html).
| `RequiresPlayMode`         | Can be applied to an assembly, fixture, or individual test to indicate that tests under its scope should (or should not) run in the Editor's Play Mode. See [RequiresPlayMode](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEngine.TestTools.RequiresPlayModeAttribute.html).
| `TestMustExpectAllLogs`    | Enforces that every log entry must be expected for a test to pass. See [TestMustExpectAllLogsAttribute](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEngine.TestTools.TestMustExpectAllLogsAttribute.html).
| `TestPlayerBuildModifier`  | Modify Player build options or split build and run. See [TestPlayerBuildModifierAttribute](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEditor.TestTools.TestPlayerBuildModifierAttribute.html).
| `TestRunCallback`          | Assembly level attribute used to subscribe a given type to updates on the test progress. See [TestRunCallbackAttribute](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEngine.TestTools.TestRunCallbackAttribute.html).
| `UnityPlatform`            | Define which platforms tests should run on. See [UnityPlatformAttribute](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEngine.TestTools.UnityPlatformAttribute.html).
| `UnitySetUp`               | Unity extension of NUnit `SetUp` to allow Unity yield instructions. See [Actions outside tests](./reference-actions-outside-tests.md).
| `UnityTearDown`            | Unity extension of NUnit `TearDown` to allow Unity yield instructions. [Actions outside tests](./reference-actions-outside-tests.md).
| `UnityTest`                | Unity extension of NUnit `Test` to allow skipping frames and Unity yield instructions. See [UnityTestAttribute](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEngine.TestTools.UnityTestAttribute.html).
