using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.TestTools.TestRunner;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.TestRun;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks;

namespace TestRun.Tasks.Player
{
    internal class SetupBuildPlayerOptionsTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            var targetPlatform = testJobData.executionSettings.targetPlatform ?? EditorUserBuildSettings.activeBuildTarget;
            var buildOptions = new BuildPlayerOptions();
            var scenes = new List<string>() { testJobData.InitTestScenePath };
            scenes.AddRange(EditorBuildSettings.scenes.Select(x => x.path));
            buildOptions.scenes = scenes.ToArray();

            buildOptions.options |= BuildOptions.Development | BuildOptions.ConnectToHost | BuildOptions.IncludeTestAssemblies | BuildOptions.StrictMode;
            buildOptions.target = targetPlatform;
#if UNITY_2021_2_OR_NEWER
            buildOptions.subtarget = EditorUserBuildSettings.GetActiveSubtargetFor(targetPlatform);
#endif

            if (EditorUserBuildSettings.waitForPlayerConnection)
            {
                buildOptions.options |= BuildOptions.WaitForPlayerConnection;
            }
            if (EditorUserBuildSettings.allowDebugging)
            {
                buildOptions.options |= BuildOptions.AllowDebugging;
            }
            if (EditorUserBuildSettings.installInBuildFolder)
            {
                buildOptions.options |= BuildOptions.InstallInBuildFolder;
            }
            else if (!testJobData.executionSettings.IsBuildOnly)
            {
                buildOptions.options |= BuildOptions.AutoRunPlayer;
            }

            var buildTargetGroup = EditorUserBuildSettings.activeBuildTargetGroup;
            buildOptions.targetGroup = buildTargetGroup;

            buildOptions.options |= PlayerLauncherBuildOptions.GetCompressionBuildOptions(buildTargetGroup, targetPlatform);

            var uniqueTempPathInProject = FileUtil.GetUniqueTempPathInProject();
            var playerDirectoryName = "PlayerWithTests";

            // Some platforms hit MAX_PATH limits during the build process, in these cases minimize the path length.
            if (ShouldReduceBuildLocationPathLength(targetPlatform))
            {
                playerDirectoryName = "PwT";
                uniqueTempPathInProject = Path.GetTempFileName();
                File.Delete(uniqueTempPathInProject);
                Directory.CreateDirectory(uniqueTempPathInProject);
            }
            var playerSavePath = testJobData.executionSettings.playerSavePath;
            if (string.IsNullOrEmpty(playerSavePath))
            {
                playerSavePath = Path.GetFullPath(uniqueTempPathInProject);
            }
            var buildLocation = Path.Combine(playerSavePath, playerDirectoryName);
            var builder = TestRunnerApi.GetPlayerBuilderFromName(testJobData.executionSettings.playerBuilderName);
            // iOS builds create a folder with Xcode project instead of an executable, therefore no executable name is added
            if (builder.AlwaysUseDirectoryForLocationPath || targetPlatform == BuildTarget.iOS)
            {
                buildOptions.locationPathName = buildLocation;
            }
            else
            {
                string extensionForBuildTarget = PostprocessBuildPlayer.GetExtensionForBuildTarget(buildTargetGroup, buildOptions.target, buildOptions.options);
                var playerExecutableName = "PlayerWithTests";
                if (!string.IsNullOrEmpty(extensionForBuildTarget))
                    playerExecutableName += $".{extensionForBuildTarget}";

                buildOptions.locationPathName = Path.Combine(buildLocation, playerExecutableName);
            }

            testJobData.PlayerBuildOptions = buildOptions;
            yield return null;
        }

        private static bool ShouldReduceBuildLocationPathLength(BuildTarget target)
        {
            switch (target)
            {
#if UNITY_2020_2_OR_NEWER
                case BuildTarget.GameCoreXboxOne:
                case BuildTarget.GameCoreXboxSeries:
#else
                case BuildTarget.XboxOne:
#endif
                case BuildTarget.WSAPlayer:
                    return true;
                default:
                    return false;
            }
        }
    }
}
