using System.Reflection;
using UnityEngine.TestTools.Utils;

namespace UnityEngine.TestTools.NUnitExtensions
{
    internal struct AssemblyWithPlatform
    {
        public IAssemblyWrapper AssemblyWrapper;
        public TestPlatform TestPlatform;

        public AssemblyWithPlatform(IAssemblyWrapper assemblyWrapper, TestPlatform testPlatform)
        {
            AssemblyWrapper = assemblyWrapper;
            TestPlatform = testPlatform;
        }
    }
}
