using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestRunner.TestLaunchers;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal class TestAdaptorFactory : ITestAdaptorFactory
    {
        private Dictionary<string, TestAdaptor> m_TestAdaptorCache = new Dictionary<string, TestAdaptor>();
        private Dictionary<string, string> m_TestUniqueNamesCache = new Dictionary<string, string>();
        private Dictionary<string, TestResultAdaptor> m_TestResultAdaptorCache = new Dictionary<string, TestResultAdaptor>();


        private string GetUniqueNameFromTestId(ITest test)
        {
            if (m_TestUniqueNamesCache.TryGetValue(test.Id, out var uniqueName))
            {
                return uniqueName;
            }

            uniqueName = test.GetUniqueName();
            m_TestUniqueNamesCache.Add(test.Id, uniqueName);
            return uniqueName;
        }

        public ITestAdaptor Create(ITest test)
        {
            var uniqueName = GetUniqueNameFromTestId(test);
            if (m_TestAdaptorCache.ContainsKey(uniqueName))
            {
                return m_TestAdaptorCache[uniqueName];
            }

            var adaptor = new TestAdaptor(test, uniqueName, test.Tests.Select(Create).ToArray());
            foreach (var child in adaptor.Children)
            {
                (child as TestAdaptor).SetParent(adaptor);
            }
            m_TestAdaptorCache[uniqueName] = adaptor;
            return adaptor;
        }

        public ITestAdaptor Create(ITest test, ITestFilter filter)
        {
            if (filter == null)
                return Create(test);

            if (!filter.Pass(test))
            {
                if (test.Parent == null)
                {
                    // Create an empty root.
                    return new TestAdaptor(test, children: new ITestAdaptor[0]);
                }

                return null;
            }

            var children = test.Tests
                .Select(c => Create(c, filter))
                .Where(c => c != null)
                .ToArray();

            var adaptor = new TestAdaptor(test, children: children);

            foreach (var child in adaptor.Children)
                (child as TestAdaptor).SetParent(adaptor);

            return adaptor;
        }

        public ITestResultAdaptor Create(ITestResult testResult)
        {
            var uniqueName = GetUniqueNameFromTestId(testResult.Test);
            if (m_TestResultAdaptorCache.ContainsKey(uniqueName))
            {
                return m_TestResultAdaptorCache[uniqueName];
            }
            var adaptor = new TestResultAdaptor(testResult, Create(testResult.Test), testResult.Children.Select(Create).ToArray());
            m_TestResultAdaptorCache[uniqueName] = adaptor;
            return adaptor;
        }

        public void ClearResultsCache()
        {
            m_TestResultAdaptorCache.Clear();
        }

        public void ClearTestsCache()
        {
            m_TestUniqueNamesCache.Clear();
            m_TestAdaptorCache.Clear();
        }
    }
}
