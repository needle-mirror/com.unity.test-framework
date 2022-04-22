# Custom attributes

As a part of Unity Test Framework's public API we provide the following attributes:

## Attributes

| Attribute                  | Description                                                  |
| -------------------------- | ------------------------------------------------------------ |
| `ParameterizedIgnore`       | A custom alternative to NUnit `Ignore` that allows ignoring tests based on parameters passed to the test method. See [ParameterizedIgnoreAttribute](xref:UnityEngine.TestTools.ParameterizedIgnoreAttribute).
| `PostBuildCleanup`         | Make changes to Unity or the file system after building. See [Setup and cleanup at build time](./reference-setup-and-cleanup.md).
| `PrebuildSetup`            | Make changes to Unity or the file system before building. See [Setup and cleanup at build time](./reference-setup-and-cleanup.md).
| `PreservedValues`          | Like NUnit `Values` this is used to provide literal arguments for an individual test parameter. See [PreservedValuesAttribute](xref:UnityEngine.TestTools.PreservedValuesAttribute).
| `RequirePlatformSupport`   | Require Player build support for the specified platforms in order to run tests. See [RequirePlayformSupportAttribute](xref:UnityEditor.TestTools.RequirePlatformSupportAttribute).
| `RequiresPlayMode`         | Can be applied to an assembly, fixture, or individual test to indicate that tests under its scope should (or should not) run in the Editor's Play Mode. See [RequiresPlayMode](xref:UnityEngine.TestTools.RequiresPlayModeAttribute).
| `TestMustExpectAllLogs`    | Enforces that every log entry must be expected for a test to pass. See [TestMustExpectAllLogsAttribute](xref:UnityEngine.TestTools.TestMustExpectAllLogsAttribute).
| `TestPlayerBuildModifier`  | Modify Player build options or split build and run. See [TestPlayerBuildModifierAttribute](xref:UnityEditor.TestTools.TestPlayerBuildModifierAttribute).
| `TestRunCallback`          | Assembly level attribute used to subscribe a given type to updates on the test progress. See [TestRunCallbackAttribute](xref:UnityEngine.TestRunner.TestRunCallbackAttribute).
| `UnityPlatform`            | Define which platforms tests should run on. See [UnityPlatformAttribute](xref:UnityEngine.TestTools.UnityPlatformAttribute).
| `UnitySetUp`               | Unity extension of NUnit `SetUp` to allow Unity yield instructions. See [Actions outside tests](./reference-actions-outside-tests.md).
| `UnityTearDown`            | Unity extension of NUnit `TearDown` to allow Unity yield instructions. [Actions outside tests](./reference-actions-outside-tests.md).
| `UnityTest`                | Unity extension of NUnit `Test` to allow skipping frames and Unity yield instructions. See [UnityTestAttribute](xref:UnityEngine.TestTools.UnityTestAttribute).
