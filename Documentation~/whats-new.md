# What's new in version 2.0

This page summarizes new features, improvements, and issues resolved in version 2.0 of Unity Test Framework. 

## Added

### Combine Edit Mode and Play Mode tests

This version removes the previous requirement to keep Edit Mode and Play Mode tests in separate assemblies by introducing the [RequiresPlayModeAttribute](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEngine.TestTools.RequiresPlayModeAttribute.html). An Editor-only test assembly can now include tests that will run in the Editor's Play Mode if given the `[RequiresPlayMode]` attribute. Likewise, a platform-specific assembly can include Edit Mode tests and exempt them from running in Play Mode if the tests are given the `[RequiresPlayMode(False)]` attribute.

### Ignore tests based on arguments

This version introduces the [ParameterizedIgnoreAttribute](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEngine.TestTools.ParameterizedIgnoreAttribute.html) which allows ignoring tests based on arguments which were passed to the test method of a [parameterized test](./reference-tests-parameterized.md).

### Async tests

This version introduces support for writing asynchronous tests with the dotnet [Task](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/task-asynchronous-programming-model) asynchronous programming model. See [Async tests](./reference-async-tests.md). Feedback on this feature is especially welcome.

## Updated

### Revised Test Runner UI

This version includes a revised Test Runner window with support for combined Edit Mode and Play Mode tests and several usability improvements:
* The former separate Edit Mode and Play Mode tabs are gone and replaced with single options for [creating a Test Assembly folder](./workflow-create-test-assembly.md) and [test script](./workflow-create-test.md). 
* Additional usability improvements make it easier to filter and [run](./workflow-run-test.md) tests. 
* The **Run Selected** option is now available when [running tests on a player](./workflow-run-playmode-test-standalone.md).

## Fixed

This version includes many bug fixes and performance improvements. For a full list of changes and updates in this version, see the Unity Test Framework package [changelog](https://docs.unity3d.com/Packages/com.unity.test-framework@2.0/changelog/CHANGELOG.html).
