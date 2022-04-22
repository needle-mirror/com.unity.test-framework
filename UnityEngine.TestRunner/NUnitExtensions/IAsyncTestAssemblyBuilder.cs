using System.Collections.Generic;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;

namespace UnityEngine.TestTools.NUnitExtensions
{
    internal interface IAsyncTestAssemblyBuilder : ITestAssemblyBuilder
    {
        IEnumerator<ITest> BuildAsync(AssemblyWithPlatform[] assemblies);
    }
}
