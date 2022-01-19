using System.Collections;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal interface IPlayerBuilder
    {
        string Name { get; }
        bool AlwaysUseDirectoryForLocationPath { get; }

        IEnumerator BuildAndRun(ExecutionSettings settings, BuildPlayerOptions buildOptions);
    }
}
