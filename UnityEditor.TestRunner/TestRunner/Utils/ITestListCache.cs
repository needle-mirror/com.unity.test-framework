using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner
{
    interface ITestListCache
    {
        ITest CachedTree { get; set; }
    }
}
