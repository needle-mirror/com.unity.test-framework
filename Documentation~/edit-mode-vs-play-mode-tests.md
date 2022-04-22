# Edit Mode vs. Play Mode tests

Before getting started with Unity Test Framework it's important to understand what Play Mode and Edit Mode tests mean in the Unity Test Framework context. 

## Edit Mode tests

**Edit Mode** tests (also known as Editor tests) are only run in the Unity Editor and have access to the Editor code in addition to the game code.

With Edit Mode tests you can test any of your [Editor extensions](https://docs.unity3d.com/Manual/ExtendingTheEditor.html) using the [UnityTest](./reference-attribute-unitytest.md) attribute. For Edit Mode tests, your test code runs in the [EditorApplication.update](https://docs.unity3d.com/ScriptReference/EditorApplication-update.html) callback loop. 

> **Note**: You can also control entering and exiting Play Mode from your Edit Mode test. This allow your test to make changes before entering Play Mode.

Edit Mode tests should meet one of the following conditions:

* They should have an [assembly definition](./workflow-create-test-assembly.md) with reference to *nunit.framework.dll* 
* Tests will be treated as Edit Mode by default if they only have the Editor as a target platform:

```assembly
    "includePlatforms": [
        "Editor"
    ],
```

* If their assembly targets other platforms, EditMode tests should be have `[RequiresPlayMode(false)]`. See [Requiring Play Mode](#requiring-play-mode) below.

## Play Mode tests

Play Mode tests allow you to exercise your game code at Runtime, as the tests run as [coroutines](https://docs.unity3d.com/ScriptReference/Coroutine.html) if marked with the `UnityTest` attribute. If you [run tests in the standard way](./workflow-run-test.md), Play Mode tests will run in the Editor's in-built [Play Mode](https://docs.unity3d.com/Manual/ConfigurableEnterPlayMode.html). You can also run Play Mode tests [in a standalone Player](./workflow-run-playmode-test-standalone.md) built for the target platform.

Play Mode tests should meet the following conditions:

* Have an [assembly definition](./workflow-create-test-assembly.md) with reference to *nunit.framework.dll*. 
* Have the test scripts located in a folder with the .asmdef file.
* The test assembly should reference an assembly that contains the code you want to test.
* Any project code you want to test must have its own assembly definition so that your test assembly can reference it.

```assembly
    "references": [
        "NewAssembly"
    ],
    "optionalUnityReferences": [
        "TestAssemblies"
   ],
    "includePlatforms": [],
```

## Requiring Play Mode

If you have a test assembly that only has the Editor as a target platform then by default Unity Test Framework runs any tests in the assembly in Edit Mode only. Likewise, if your assembly targets a platform other than the Editor, for example Windows 64-bit, Unity Test Framework assumes these are Play Mode tests and will run them in the Editor's Play Mode by default.

You can override these defaults using the [RequirePlayModeAttribute](). If tests are part of an Editor-only assembly, you can annotate them with `[RequiresPlayMode]` to make them run in the Editor's Play Mode. If you have a platform-specific assembly but you don't want your tests to run in the Editor's Play Mode, you can exclude them from running in Play Mode with `[RequiresPlayMode(false)]`.

### Where will my test run?

The following table summarizes where your test will run based on assembly configuration, `RequiresPlayMode` value, and **Run Location** option:

| Assembly reference      | RequiresPlayMode       | Run Location    | Runs in          |
| ------------------------| ---------------------- | --------------- | -----------------|
| Editor only             | False or Not Specified | In Editor       | Editor Edit Mode |
|                         | True                   | In Editor       | Editor Play Mode |
| Specific platform(s)    | False                  | In Editor       | Editor Edit Mode |
|                         |                        | On Player       | Player           |
|                         | True or Not Specified  | In Editor       | Editor Play Mode |
|                         |                        | On Player       | Player           |

In summary: an Editor assembly makes your tests Edit Mode by default unless overridden by `[RequiresPlayMode]`. A platform assembly makes your tests Play Mode by default, unless overridden by `[RequiresPlayMode(false)]`. Tests run in a Player will always run in the Player.

## References and builds

Unity Test Framework adds a reference to test assemblies in the Assembly Definition file but does not include any other references (e.g., to other scripting assemblies in your Unity project). To test other assemblies, you need to add them to the assembly definition yourself. For details on adding assembly references, see [Assembly Definition](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html).
We recommend organizing your tests into separate assemblies and using assembly definitions files to selectively expose them to the code they test. This way you will have more control over which assemblies need to reference test related dlls and avoid having unnecessary assemblies in your builds.

## Recommendations

### Attributes

Use the [NUnit](http://www.nunit.org/) `Test` attribute instead of the `UnityTest` attribute, unless you need to [yield special instructions](./reference-custom-yield-instructions.md), in Edit Mode, or if you need to skip a frame or wait for a certain amount of time in Play Mode.

### References

It is possible for your Test Assemblies to reference the test tools in `UnityEngine.TestRunner` and `UnityEditor.TestRunner`. The latter is only available in Edit Mode. You can specify these references in the `Assembly Definition References` on the Assembly Definition.
