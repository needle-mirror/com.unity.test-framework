using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditor.TestTools.TestRunner;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using IPlayerBuilder = UnityEditor.TestTools.TestRunner.Api.IPlayerBuilder;

namespace TestRun.Tasks.Player
{
    internal class DefaultPlayerBuilder : IPlayerBuilder
    {
        internal static string k_Name = "DefaultPlayerBuilder";
        public string Name { get; } = k_Name;
        public bool AlwaysUseDirectoryForLocationPath
        {
            get => false;
        }

        public IEnumerator BuildAndRun(ExecutionSettings settings, BuildPlayerOptions buildOptions)
        {
            PrintBuildOptions(buildOptions);

#if !UNITY_2021_2_OR_NEWER
            //only flip connect to host if we are under 2021.2
            if (buildOptions.target == BuildTarget.Android)
            {
                buildOptions.options &= ~BuildOptions.ConnectToHost;
            }
#endif
            // For now, so does Lumin
            if (buildOptions.target == BuildTarget.Lumin)
            {
                buildOptions.options &= ~BuildOptions.ConnectToHost;
            }

            var result = BuildPipeline.BuildPlayer(buildOptions);
            if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError(result.SummarizeErrors());
                throw new TestLaunchFailedException("Player build failed");
            }

            yield return null;
        }

        private void PrintBuildOptions(BuildPlayerOptions buildOptions)
        {
            var target = string.Concat("BuildTarget: ", buildOptions.target.ToString());
            var locationPathName = string.Concat("BuildPlayerLocation: ", buildOptions.locationPathName);
            var options = string.Concat("Options: ", buildOptions.options.ToString());
            var scenes = string.Concat("Scenes: ", string.Join(",", buildOptions.scenes));
            var buildOptionInfo = string.Join(Environment.NewLine, new[] {target, locationPathName, options, scenes});
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "Building player with following options:\n{0}", buildOptionInfo);
        }
    }
}
