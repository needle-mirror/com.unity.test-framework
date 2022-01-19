using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner
{
    internal class TestListJob
    {
        private CachingTestListProvider m_TestListProvider;
        private ITestFilter m_Filter;
        private Action<ITestAdaptor> m_Callback;
        private IEnumerator<ITestAdaptor> m_ResultEnumerator;
        public TestListJob(CachingTestListProvider testListProvider, ITestFilter filter, Action<ITestAdaptor> callback)
        {
            m_TestListProvider = testListProvider;
            m_Filter = filter;
            m_Callback = callback;
        }

        public void Start()
        {
            m_ResultEnumerator = m_TestListProvider.GetTestListAsync(m_Filter);
            EditorApplication.update += EditorUpdate;
        }

        private void EditorUpdate()
        {
            if (!m_ResultEnumerator.MoveNext())
            {
                m_Callback(m_ResultEnumerator.Current);
                EditorApplication.update -= EditorUpdate;
            }
        }
    }
}
