using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Scripting.ScriptCompilation;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace UnityEditor.TestTools.TestRunner
{
    internal class EditorLoadedTestAssemblyProvider : IEditorLoadedTestAssemblyProvider
    {
        private const string k_NunitAssemblyName = "nunit.framework";
        private const string k_TestRunnerAssemblyName = "UnityEngine.TestRunner";
        internal const string k_PerformanceTestingAssemblyName = "Unity.PerformanceTesting";

        private readonly IEditorAssembliesProxy m_EditorAssembliesProxy;
        private readonly ScriptAssembly[] m_AllEditorScriptAssemblies;
        private readonly PrecompiledAssembly[] m_AllPrecompiledAssemblies;

        public EditorLoadedTestAssemblyProvider(IEditorCompilationInterfaceProxy compilationInterfaceProxy, IEditorAssembliesProxy editorAssembliesProxy)
        {
            m_EditorAssembliesProxy = editorAssembliesProxy;
            m_AllEditorScriptAssemblies = compilationInterfaceProxy.GetAllEditorScriptAssemblies();
            m_AllPrecompiledAssemblies = compilationInterfaceProxy.GetAllPrecompiledAssemblies();
        }

        public List<IAssemblyWrapper> GetAssembliesGroupedByType(TestPlatform mode)
        {
            var assemblies = GetAssembliesGroupedByTypeAsync(mode);
            while (assemblies.MoveNext())
            {
            }

            return assemblies.Current.Where(pair => mode.IsFlagIncluded(pair.Key)).SelectMany(pair => pair.Value).ToList();
        }

        public IEnumerator<IDictionary<TestPlatform, List<IAssemblyWrapper>>> GetAssembliesGroupedByTypeAsync(TestPlatform mode)
        {
            IAssemblyWrapper[] loadedAssemblies = m_EditorAssembliesProxy.loadedAssemblies;

            IDictionary<TestPlatform, List<IAssemblyWrapper>> result = new Dictionary<TestPlatform, List<IAssemblyWrapper>>
            {
                {TestPlatform.EditMode, new List<IAssemblyWrapper>() },
                {TestPlatform.PlayMode, new List<IAssemblyWrapper>() }
            };
            var filteredAssemblies = FilterAssembliesWithTestReference(loadedAssemblies);

            foreach (var loadedAssembly in filteredAssemblies)
            {
                var assemblyName = new FileInfo(loadedAssembly.Location).Name;
                var scriptAssemblies = m_AllEditorScriptAssemblies.Where(x => x.Filename == assemblyName).ToList();
                var precompiledAssemblies = m_AllPrecompiledAssemblies.Where(x => new FileInfo(x.Path).Name == assemblyName).ToList();
                if (scriptAssemblies.Count < 1 && precompiledAssemblies.Count < 1)
                {
                    continue;
                }

                var assemblyFlags = scriptAssemblies.Any() ? scriptAssemblies.Single().Flags : precompiledAssemblies.Single().Flags;
                var assemblyType = (assemblyFlags & AssemblyFlags.EditorOnly) == AssemblyFlags.EditorOnly ? TestPlatform.EditMode : TestPlatform.PlayMode;
                result[assemblyType].Add(loadedAssembly);
                yield return null;
            }

            yield return result;
        }

        private IAssemblyWrapper[] FilterAssembliesWithTestReference(IAssemblyWrapper[] loadedAssemblies)
        {
            var resultsCache = new Dictionary<IAssemblyWrapper, bool>();
            var loadedAssembliesDict = loadedAssemblies.ToDictionary(asm => asm.Name.Name, asm => asm);
            return loadedAssemblies
	            .Where(assembly => FilterAssemblyForTestReference(assembly, loadedAssembliesDict, resultsCache))
	            .ToArray();
        }

        // This method can easily become a performance bottleneck in big projects,
        // so it needs to stay optimized in terms of computational complexity.
        // This is why we use heavy caching of results and early returns
        // as much as possible.
        private bool FilterAssemblyForTestReference(
            IAssemblyWrapper assemblyToFilter,
            IReadOnlyDictionary<string, IAssemblyWrapper> loadedAssemblies,
            IDictionary<IAssemblyWrapper, bool> resultsCache)
        {
            if (resultsCache.TryGetValue(assemblyToFilter, out var existingResult))
            {
                return existingResult;
            }

            foreach (var reference in assemblyToFilter.GetReferencedAssemblies())
            {
                if (IsTestReference(reference) ||
	                (loadedAssemblies.TryGetValue(reference.Name, out var referencedAssembly) &&
	                FilterAssemblyForTestReference(referencedAssembly, loadedAssemblies, resultsCache)))
                 {
	                resultsCache[assemblyToFilter] = true;
	                return true;
                 }
            }

            resultsCache[assemblyToFilter] = false;
            return false;
        }

        private static bool IsTestReference(AssemblyName assemblyName)
        {
            return assemblyName.Name == k_NunitAssemblyName ||
                   assemblyName.Name == k_TestRunnerAssemblyName ||
                   assemblyName.Name == k_PerformanceTestingAssemblyName;
        }
    }
}
