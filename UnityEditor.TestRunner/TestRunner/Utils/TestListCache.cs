using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestRunner.TestLaunchers;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner
{
    internal class TestListCache : ITestListCache
    {
        private static ITest _cachedTree;

        public ITest CachedTree
        {
            get { return _cachedTree; }
            set { _cachedTree = value; }
        }
    }
}
