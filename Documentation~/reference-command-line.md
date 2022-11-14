# Test Framework command line arguments

This section lists the Unity Test Framework command line arguments for Unity. For an example command to run tests on the command line, see [Running tests from the command line](./workflow-run-test.md#running-tests-from-the-command-line).

### forgetProjectPath

Don't save your current **Project** into the Unity launcher/hub history.

### runTests

Runs tests in the Project. This argument is required to run any tests.

### testCategory

A semicolon-separated list of test categories to include in the run. A semi-colon separated list should be formatted as a string enclosed in quotation marks, e.g. `testCategory "firstCategory;secondCategory"`. If using both `testFilter` and `testCategory`, then only test that matches both are run. This argument supports negation using '!'. If using '!MyCategory' then no tests with the 'MyCategory' category will be included in the run.

### testFilter

A semicolon-separated list of test names to run, or a regular expression pattern to match tests by their full name. A semi-colon separated list should be formatted as a string enclosed in quotation marks, e.g. `testFilter "Low;Medium"`. This argument supports negation using '!'. If using the test filter '!MyNamespace.Something.MyTest', then all tests except that test will be run.
It is also possible to run a specific variation of a parameterized test like so: `"ClassName\.MethodName\(Param1,Param2\)"`

### testPlatform

The platform to run tests on. Accepted values:

* **EditMode**
    * Edit Mode tests. Equivalent to running tests with the **Run Location > In Editor ** option and only the **EditMode* checkbox selected in the Test Runner window.
* **PlayMode**
    * Play Mode tests that run in the Editor. Equivalent to running tests with the **Run Location > In Editor** options and only the **PlayMode** checkbox selected in the Test Runner window.
* Any value from the [BuildTarget](https://docs.unity3d.com/ScriptReference/BuildTarget.html) enum.
    * Play Mode tests that run on a player built for the specified platform. Equivalent to using the **Run Location > On Player (`<target_platform>`)** option in the Test Runner window.

> **Note**: If no value is specified for this argument, tests run in Edit Mode.

### requiresPlayMode

Filers whether to run tests that require Play Mode.
* requiresPlayMode=true runs all tests that require Play Mode.
* requiresPlayMode=false runs all tests that do not require Play Mode.
* not specifying the parameters runs tests regardless of whether they require Play Mode or not.

### assemblyType

Filters the tests on an assembly type. Either **EditorOnly** or **EditorAndPlatforms**.

### assemblyNames

A semicolon-separated list of test assemblies to include in the run. A semi-colon separated list should be formatted as a string enclosed in quotation marks, e.g. `assemblyNames "firstAssembly;secondAssembly"`.

### testNames

A semicolon-separeted list of full name of the tests to match the filter. This is usually in the format FixtureName.TestName. If the test has test arguments, then include them in parenthesis. e.g. `MyTestClass2.MyTestWithMultipleValues(1)`.

### testResults

The path where Unity should save the result file. By default, Unity saves it in the Project’s root folder. Test results follow the XML format as defined by NUnit, see the [NUnit documentation](https://docs.nunit.org/articles/nunit/technical-notes/usage/Test-Result-XML-Format.html). There is currently no common definition for exit codes reported by individual Unity components under test. The best way to understand the source of a problem is the content of error messages and stack traces.

### playerHeartbeatTimeout

The time, in seconds, the editor should wait for heartbeats after starting a test run on a player. This defaults to 10 minutes.

### runSynchronously

If included, the test run will run tests synchronously, guaranteeing that all tests runs in one editor update call. Note that this is only supported for EditMode tests, and that tests which take multiple frames (i.e. `[UnityTest]` tests, or tests with `[UnitySetUp]` or `[UnityTearDown]` scaffolding) will be filtered out.

### buildPlayerPath
The path where Unity should save the built player with tests folder. By default it creates a temporary folder inside the project path.

### testSettingsFile 

Path to a *TestSettings.json* file that allows you to set up extra options for your test run. An example of the *TestSettings.json* file could look like this:

```json
{
  "scriptingBackend":"WinRTDotNET",
  "Architecture":null,
  "apiProfile":0
}
```

#### apiProfile

The .Net compatibility level. Set to one of the following values:  

- 1 - .Net 2.0 
- 2 - .Net 2.0 Subset 
- 3 - .Net 4.6 
- 5 - .Net micro profile (used by Mono scripting backend if **Stripping Level** is set to **Use micro mscorlib**) 
- 6 - .Net Standard 2.0 

#### appleEnableAutomaticSigning

Sets option for automatic signing of Apple devices.

#### appleDeveloperTeamID 

Sets the team ID for the apple developer account.

#### architecture

Target architecture for Android. Set to one of the following values: 

* None = 0
* ARMv7 = 1
* ARM64 = 2
* X86 = 4
* All = 4294967295

#### iOSManualProvisioningProfileType

Set to one of the following values: 

* 0 - Automatic 
* 1 - Development 
* 2 - Distribution iOSManualProvisioningProfileID

#### iOSTargetSDK

Target SDK for iOS. Set to one of the following values, which should be given as a string literal enclosed in quotes:

* DeviceSDK
* SimulatorSDK

#### tvOSTargetSDK

Target SDK for tvOS. Set to one of the following values, which should be given as a string literal enclosed in quotes:

* DeviceSDK
* SimulatorSDK

#### scriptingBackend

 Set to one of the following values, which should be given as a string literal enclosed in quotes:

- Mono2x
- IL2CPP
- WinRTDotNET

#### playerGraphicsAPI

 Set graphics API that will be used during test execution in the player. Value can be any [GraphicsDeviceType](https://docs.unity3d.com/ScriptReference/Rendering.GraphicsDeviceType.html) as a string literal enclosed in quotes. Value will only be set if it is supported on the target platform.

### androidAppBundle

A boolean setting that allows to build an Android App Bundle (AAB) instead of APK for tests.

### orderedTestListFile
Path to a *.txt* file which contains a list of full test names you want to run in the specified order. The tests should be seperated by new lines and if they have parameters, these should be specified as well. A list of the file could look like this:
```
Unity.Framework.Tests.OrderedTests.NoParameters
Unity.Framework.Tests.OrderedTests.ParametrizedTestA(3,2)
Unity.Framework.Tests.OrderedTests.ParametrizedTestB(\"Assets/file.fbx\")
Unity.Framework.Tests.OrderedTests.ParametrizedTestC(System.String[],\"foo.fbx\")
Unity.Framework.Tests.OrderedTests.ParametrizedTestD(1.0f)
Unity.Framework.Tests.OrderedTests.ParametrizedTestE(null)
Unity.Framework.Tests.OrderedTests.ParametrizedTestF(False, 1)
Unity.Framework.Tests.OrderedTests.ParametrizedTestG(float.NaN)
Unity.Framework.Tests.OrderedTests.ParametrizedTestH(SomeEnum)
```
