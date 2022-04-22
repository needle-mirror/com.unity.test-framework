# Unity Test Framework Features

## Editor UI integration

The Test Runner window integrated into the Editor (menu: **Window** > **General** > **Test Runner**) allows you to create and run your tests and the Inspector window lets you configure your test assemblies.

![Unity Test Runner window](./images/test-runner-window.png)

## Test in Edit Mode and Play Mode

Unity Test Framework supports two test modes: Edit Mode tests to test your Editor extensions and game logic and Play Mode tests to test your Runtime game code. See [Edit Mode vs. Play Mode tests](./edit-mode-vs-play-mode-tests.md) for an overview.

## Custom attributes and NUnit extensions

Unity Test Framework offers a range of [custom Unity attributes](./reference-custom-attributes.md) that supplement or extend NUnit APIs:

### Skip frames and yield instructions in tests

Unity Test Framework extends the NUnit `Test` attribute with a custom `UnityTest` attribute which allows you to skip frames and yield instructions for the Editor. See [UnityTestAttribute](xref:UnityEngine.TestTools.UnityTestAttribute) in the Scripting API.

### Setup and Cleanup at build time

Unity Test Framework provides pre-build setup and cleanup attributes to allow you to make changes to Unity or the file system before tests build and clean them up after. See [Setup and cleanup at build time](./reference-setup-and-cleanup.md).

### Actions outside tests

Unity Test Framework extends the NUnit options for performing actions in the setup and teardown phase with custom Unity attributes that allow yield instructions and an outer test action interface. See [Actions outside tests](./reference-actions-outside-tests.md).

### Equality comparers

Unity Test Framework offers a range of custom equality comparers for Color, Float, Quaternion, Vector2, Vector3, and Vector 4 comparisons. See [UnityEngine.TestTools.Utils](xref:UnityEngine.TestTools.Utils) in the Scripting API.

### Custom yield instructions

Unity Test Framework provides custom yield instructions so your Edit Mode tests can instruct Unity to enter or exit Play Mode, recompile scripts, or wait for a domain reload. See [IEditModeTestYieldInstruction](xref:UnityEngine.TestTools.IEditModeTestYieldInstruction) in the scripting API.

### Custom assertion

Unity Test Framework offers a custom assertion that allows your tests to expect specific messages in the Unity logs. See [LogAssert](xref:UnityEngine.TestTools.LogAssert) in the Scripting API.

### Custom constraints

Unity Test Framework extends the NUnit `Assert.That` mechanism with our own custom constraint type. See [Constraints](xref:UnityEngine.TestTools.Constraints) in the Scripting API.

### Parameterized tests

Unity Test Framework supports parameterized tests for data-driven testing. See [Parameterized tests](./reference-tests-parameterized.md).

### Monobehavior tests

Unity Test Framework offers a coroutine wrapper to help you test Monobehaviors. See [MonobehaviourTest](xref:UnityEngine.TestTools.MonoBehaviourTest`1) in the Scripting API.

### Run tests programmatically

Unity Test Framework provides a custom ScriptableObject with which you can retrieve and run tests programmatically from code in your project or another package. The API includes classes to filter which tests to run and to receive callbacks during the test run. See the [TestRunnerApi](xref:UnityEditor.TestTools.TestRunner.Api) Scripting API.
