using System;
using System.Collections;
using System.Linq;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class BuildTestTreeTask : TestTaskBase
    {
        public BuildTestTreeTask()
        {
            RerunAfterResume = true;
        }

        internal IEditorLoadedTestAssemblyProvider m_testAssemblyProvider = new EditorLoadedTestAssemblyProvider(new EditorCompilationInterfaceProxy(), new EditorAssembliesProxy());
        internal IAsyncTestAssemblyBuilder m_testAssemblyBuilder = new UnityTestAssemblyBuilder();
        internal ICallbacksDelegator m_CallbacksDelegator = CallbacksDelegator.instance;

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.testTree != null)
            {
                yield break;
            }

            var assembliesEnumerator = m_testAssemblyProvider.GetAssembliesAsync();
            while (assembliesEnumerator.MoveNext())
            {
                yield return null;
            }

            if (assembliesEnumerator.Current == null)
            {
                throw new Exception("Assemblies not retrieved.");
            }

            var assemblies = assembliesEnumerator.Current;
            var enumerator = m_testAssemblyBuilder.BuildAsync(assemblies.ToArray());
            while (enumerator.MoveNext())
            {
                yield return null;
            }

            var testList = enumerator.Current;
            if (testList == null)
            {
                throw new Exception("Test list not retrieved.");
            }

            testList.ParseForNameDuplicates();
            testJobData.testTree = testList;
            m_CallbacksDelegator.TestTreeRebuild(testList);
        }
    }
}
