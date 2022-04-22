using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine.TestRunner.TestLaunchers;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal interface ITestAdaptorFactory
    {
        ITestAdaptor Create(ITest test);
        ITestAdaptor Create(ITest test, ITestFilter filter);
        ITestResultAdaptor Create(ITestResult testResult);
        void ClearResultsCache();
        void ClearTestsCache();
    }
}
