using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.Scripting.ScriptCompilation;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;
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

        public IList<AssemblyWithPlatform> GetAssemblies()
        {
            var assemblies = GetAssembliesAsync();
            while (assemblies.MoveNext())
            {
            }

            return assemblies.Current;
        }

        public IEnumerator<IList<AssemblyWithPlatform>> GetAssembliesAsync()
        {
            IAssemblyWrapper[] loadedAssemblies = m_EditorAssembliesProxy.loadedAssemblies;
            var filteredAssemblies = FilterAssembliesWithTestReference(loadedAssemblies);
            var result = new List<AssemblyWithPlatform>();

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
                result.Add(new AssemblyWithPlatform(loadedAssembly, assemblyType));
                yield return null;
            }

            yield return result;
        }

        private IAssemblyWrapper[] FilterAssembliesWithTestReference(IAssemblyWrapper[] loadedAssemblies)
        {
            var filteredResults = new Dictionary<IAssemblyWrapper, bool>();
            var assembliesByName = new Dictionary<string, IAssemblyWrapper>();
            foreach (var assembly in loadedAssemblies)
            {
                if (!assembliesByName.ContainsKey(assembly.Name.Name))
                    assembliesByName.Add(assembly.Name.Name, assembly);
            }

            foreach (var assembly in loadedAssemblies)
            {
                FilterAssemblyForTestReference(assembly, assembliesByName, filteredResults);
            }

            return filteredResults.Where(pair => pair.Value).Select(pair => pair.Key).ToArray();
        }

        private void FilterAssemblyForTestReference(IAssemblyWrapper assemblyToFilter, Dictionary<string, IAssemblyWrapper> assembliesByName, IDictionary<IAssemblyWrapper, bool> filterResults)
        {
            if (filterResults.ContainsKey(assemblyToFilter))
            {
                return;
            }

            // Default to false, as well as avoid circular dependencies creating stack overflow
            filterResults[assemblyToFilter] = false;

            var references = assemblyToFilter.GetReferencedAssemblies();
            if (references.Any(IsTestReference))
            {
                filterResults[assemblyToFilter] = true;
                return;
            }

            foreach (var reference in references)
            {
                if (!assembliesByName.TryGetValue(reference.Name, out var referencedAssembly))
                    continue;

                FilterAssemblyForTestReference(referencedAssembly, assembliesByName, filterResults);

                if (filterResults[referencedAssembly])
                {
                    filterResults[assemblyToFilter] = true;
                    return;
                }
            }
        }

        private static bool IsTestReference(AssemblyName assemblyName)
        {
            return assemblyName.Name == k_NunitAssemblyName ||
                assemblyName.Name == k_TestRunnerAssemblyName ||
                assemblyName.Name == k_PerformanceTestingAssemblyName;
        }
    }
}
