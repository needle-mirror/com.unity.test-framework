using System.Collections.Generic;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;
using UnityEngine.TestTools.Utils;

namespace UnityEditor.TestTools.TestRunner
{
    internal interface IEditorLoadedTestAssemblyProvider
    {
        IList<AssemblyWithPlatform> GetAssemblies();
        IEnumerator<IList<AssemblyWithPlatform>> GetAssembliesAsync();
    }
}
