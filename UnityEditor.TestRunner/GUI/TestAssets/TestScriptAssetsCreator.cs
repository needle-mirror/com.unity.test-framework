using System;
using System.IO;

namespace UnityEditor.TestTools.TestRunner.GUI.TestAssets
{
    /// <inheritdoc />
    class TestScriptAssetsCreator : ITestScriptAssetsCreator
    {
        const string k_AssemblyDefinitionEditModeTestTemplate = "92-Assembly Definition-NewEditModeTestAssembly.asmdef.txt";
        internal const string assemblyDefinitionTestTemplate = "92-Assembly Definition-NewTestAssembly.asmdef.txt";

        internal const string resourcesTemplatePath = "Resources/ScriptTemplates";
        internal const string testScriptTemplate = "83-C# Script-NewTestScript.cs.txt";

        internal const string defaultNewTestAssemblyFolderName = "Tests";
        internal const string defaultNewTestScriptName = "NewTestScript.cs";

        static IFolderPathTestCompilationContextProvider s_FolderPathCompilationContext;
        static IActiveFolderTemplateAssetCreator s_ActiveFolderTemplateAssetCreator;
        static ITestScriptAssetsCreator s_Instance;

        internal static IFolderPathTestCompilationContextProvider FolderPathContext
        {
            private get => s_FolderPathCompilationContext ?? (s_FolderPathCompilationContext = new FolderPathTestCompilationContextProvider());
            set => s_FolderPathCompilationContext = value;
        }

        internal static IActiveFolderTemplateAssetCreator ActiveFolderTemplateAssetCreator
        {
            private get => s_ActiveFolderTemplateAssetCreator ?? (s_ActiveFolderTemplateAssetCreator = new ActiveFolderTemplateAssetCreator());
            set => s_ActiveFolderTemplateAssetCreator = value;
        }

        internal static ITestScriptAssetsCreator Instance => s_Instance ?? (s_Instance = new TestScriptAssetsCreator());

        static string ActiveFolderPath => ActiveFolderTemplateAssetCreator.GetActiveFolderPath();
        static string ScriptTemplatesResourcesPath => Path.Combine(EditorApplication.applicationContentsPath, resourcesTemplatePath);
        static string ScriptTemplatePath => Path.Combine(ScriptTemplatesResourcesPath, testScriptTemplate);

        /// <inheritdoc />
        public void AddNewFolderWithTestAssemblyDefinition(bool isEditorOnly = false)
        {
            var assemblyDefinitionTemplate = isEditorOnly ? k_AssemblyDefinitionEditModeTestTemplate : assemblyDefinitionTestTemplate;
            ActiveFolderTemplateAssetCreator.CreateFolderWithTemplates(defaultNewTestAssemblyFolderName, assemblyDefinitionTemplate);
        }

        /// <inheritdoc />
        public void AddNewTestScript()
        {
            var destPath = Path.Combine(ActiveFolderTemplateAssetCreator.GetActiveFolderPath(), defaultNewTestScriptName);
            ActiveFolderTemplateAssetCreator.CreateScriptAssetFromTemplateFile(destPath, ScriptTemplatePath);
        }

        /// <inheritdoc />
        public bool ActiveFolderContainsTestAssemblyDefinition()
        {
            return FolderPathContext.FolderPathBelongsToCustomTestAssembly(ActiveFolderPath);
        }

        /// <inheritdoc />
        public bool TestScriptWillCompileInActiveFolder()
        {
            return FolderPathContext.TestScriptWillCompileInFolderPath(ActiveFolderPath);
        }
    }
}
