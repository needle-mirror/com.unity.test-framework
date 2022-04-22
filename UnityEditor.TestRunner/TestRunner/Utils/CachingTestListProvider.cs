using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.Api.Analytics;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner
{
    internal class CachingTestListProvider
    {
        private readonly ITestListProvider m_InnerTestListProvider;
        private readonly ITestListCache m_TestListCache;
        private readonly ITestAdaptorFactory m_TestAdaptorFactory;
        public CachingTestListProvider(ITestListProvider innerTestListProvider, ITestListCache testListCache, ITestAdaptorFactory testAdaptorFactory)
        {
            m_InnerTestListProvider = innerTestListProvider;
            m_TestListCache = testListCache;
            m_TestAdaptorFactory = testAdaptorFactory;
        }

        public IEnumerator<ITestAdaptor> GetTestListAsync(ITestFilter filter)
        {
            if (m_TestListCache.CachedTree == null)
            {
                var test = m_InnerTestListProvider.GetTestListAsync();
                while (test.MoveNext())
                {
                    yield return null;
                }

                test.Current.ParseForNameDuplicates();
                m_TestListCache.CachedTree = test.Current;
                AnalyticsReporter.AnalyzeTestTreeAndReport(test.Current);
            }

            yield return m_TestAdaptorFactory.Create(m_TestListCache.CachedTree, filter);
        }
    }
}
