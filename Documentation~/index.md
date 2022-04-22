# Unity Test Framework overview

The Unity Test Framework allows you to test code in both **Edit Mode** and **Play Mode**, and also on target platforms such as [Standalone](https://docs.unity3d.com/Manual/Standalone.html), Android, iOS, etc.

This package provides a standard test framework for Unity users and developers so that both benefit from the same features and can write tests the same way. 

UTF includes a custom integration of NUnit (based on NUnit 3.5), the open-source unit testing library for .Net languages, adapted to work with Unity. For more information about NUnit, see the [official NUnit website](http://www.nunit.org/) and the [NUnit documentation](https://docs.nunit.org/). 

## Using this documentation

The **Manual** outlines key features, concepts, and workflows for creating and running tests via the Test Runner window. The [Scripting API](../api/index.html) contains descriptions and code examples for the public APIs. You can browse the Scripting API freely or review the features in the Manual to discover what API members you might be interested in. 

## Prerequisite knowledge

There are some concepts which this documentation assumes you're familiar with:

### Assemblies

Assemblies are an important foundational concept in Unity Test Framework. We group tests into Test Assemblies and configure the assembly definitions to reference platforms the tests should run on and to selectively reference the project assemblies we want to test. You should be familiar with the [documentation on Assembly Definitions](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html) in Unity and how to configure them via the Inspector or, if you prefer, by editing assembly definition (`.asmdef`) JSON files directly.

### NUnit

UTF includes a custom version of NUnit adapted to work with Unity. It's important to note that a lot of the APIs you use when writing UTF tests are NUnit APIs. Some of your tests may not need to use any of the Unity-specific APIs at all. Before you try using UTF you should familiarize yourself with NUnit and you should use the UTF and [NUnit documentation](https://docs.nunit.org/) side by side.

### TDD and unit testing

This documentation is not intended as a general introduction to test driven development (TDD) or unit testing. Although UTF is intended for use by our users as well as our developers, we assume some familiarity with unit testing conventions in C# or a similar language such as:

* The Arrange, Act, Assert (AAA) pattern whereby we set up tests, perform an action, and expect a result.
* Using attributes to annotate methods and designate them to run at different phases of the test execution.
* Implementing interfaces and callbacks.

See the [Microsoft documentation](https://docs.microsoft.com/en-us/visualstudio/test/unit-test-basics?view=vs-2019) for an introduction to unit test fundamentals in Visual Studio, much of which is similar to the way we create, write, and run UTF tests in Unity.

## Installing Unity Test Framework

To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html).

> **Note**: Search for the Test Framework package. In Unity 2019.2 and higher, you may need to enable the package before use. 

## Requirements

This version of the Unity Test Framework is compatible with the following versions of the Unity Editor:

* 2019.2 and later.

## Known limitations

Unity Test Framework version 2.0.1 includes the following known limitations:

* The `UnityTest` attribute does not support WebGL and WSA platforms.
* The `UnityTest` attribute does not support [Parameterized tests](https://docs.nunit.org/articles/nunit/technical-notes/usage/Parameterized-Tests.html) (except for `ValueSource`).
* The `UnityTest` attribute does not support the `NUnit` [Repeat](https://docs.nunit.org/articles/nunit/writing-tests/attributes/repeat.html) attribute.
* Nested test fixture cannot run from the Editor UI. 
* When using the `NUnit` [Retry](https://docs.nunit.org/articles/nunit/writing-tests/attributes/retry.html) attribute in PlayMode tests, it throws `InvalidCastException`.
* [Parameterized tests](./reference-tests-parameterized.md) do not work when tests are generated at runtime.
* The test tree in the UI is built before the run and only displays tests matching the tree at that time. Any test completely behind a define (e.g. `if #UNITY_ANDROID`) will still run, but the result will not show up in the Editor. The workaround for this is to have only the body of the test method behind the define.
