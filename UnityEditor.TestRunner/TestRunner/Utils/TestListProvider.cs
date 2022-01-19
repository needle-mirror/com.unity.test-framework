using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;

namespace UnityEditor.TestTools.TestRunner
{
    internal class TestListProvider : ITestListProvider
    {
        private readonly EditorLoadedTestAssemblyProvider m_AssemblyProvider;
        private readonly UnityTestAssemblyBuilder m_AssemblyBuilder;

        public TestListProvider(EditorLoadedTestAssemblyProvider assemblyProvider, UnityTestAssemblyBuilder assemblyBuilder)
        {
            m_AssemblyProvider = assemblyProvider;
            m_AssemblyBuilder = assemblyBuilder;
        }

        public IEnumerator<ITest> GetTestListAsync()
        {
            var assembliesTask = m_AssemblyProvider.GetAssembliesAsync();
            while (assembliesTask.MoveNext())
            {
                yield return null;
            }

            var test =  m_AssemblyBuilder.BuildAsync(assembliesTask.Current.ToArray());
            while (test.MoveNext())
            {
                yield return null;
            }

            yield return test.Current;
        }
    }
}
