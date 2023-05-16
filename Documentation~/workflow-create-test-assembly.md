# Workflow: **How to create a new test assembly** 

Unity Test Framework looks for a test inside any assembly that references [NUnit](http://www.nunit.org/). We refer to such assemblies as **Test Assemblies**. **Play Mode** and **Edit Mode** tests need to be in separate assemblies.

The [Test Runner](./getting-started.md) UI helps you set up **TestAssemblies**:

1. Select the **Assets** folder in your Project window.
2. Open the **Test Runner** window (menu: **Window** > **General** > **Test Runner**).
3. In the **Test Runner** window, the **EditMode** tab should be selected by default. Click **Create EditMode Test Assembly Folder**. 

![Test Runner window EditMode tab](./images/editmode-tab.png)

This creates a *Tests* folder in your project Assets with a corresponding `.asmdef` file with the required references. You can change the name of the new [Assembly Definition](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html) and press Enter to accept it.

![New Test folder and assembly file](./images/tests-folder-assembly.png)

Click on the assembly definition file to inspect it in the Inspector window. You'll see that it has references to **nunit.framework.dll** under Assembly References, and to **UnityEngine.TestRunner** and **UnityEditor.TestRunner** under Assembly Definition References. This tells Unity Test Framework that this is a test assembly.

The checkbox selections under **Platforms** determine which platforms the test assembly can run on. Assemblies created through the **Test Runner** target the **Editor** only by default. **Any Platform** or a specific platform other than **Editor** makes the tests you add run in Play Mode by default.

> **Note**: The **UnityEditor.TestRunner** reference is only available for [Edit Mode tests](./edit-mode-vs-play-mode-tests.md#edit-mode-tests).

![Assembly definition import settings](./images/import-settings.png)

## Reference other assemblies in your Test Assemblies

To reference other assemblies from your Test Assemblies:

1. Create your Test Assembly as described above.
2. Create an Assembly Definition for the scripts you want to reference, refer to [Create an Assembly Definition asset](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html#create-asmdef).
3. Add a reference to the Assembly Definition created in Step 2 to your Test Assembly, refer to [Referencing another assembly](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html#reference-another-assembly).

## Additional resources

Refer to the Unity Manual page on [Assembly definitions](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html) for more detail on using Assembly Definition files to manage references.