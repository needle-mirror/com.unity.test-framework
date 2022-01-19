using System.Text;

namespace UnityEditor.TestTools.TestRunner
{
    internal class PlayerLauncherBuildOptions
    {
        public BuildPlayerOptions BuildPlayerOptions;
        public string PlayerDirectory;

        // Used by Playmode BuildConfiguration extension
        internal static BuildOptions GetCompressionBuildOptions(BuildTargetGroup buildTargetGroup, BuildTarget buildTarget)
        {
            if (PostprocessBuildPlayer.SupportsLz4Compression(buildTargetGroup, buildTarget))
            {
                if (EditorUserBuildSettings.GetCompressionType(buildTargetGroup) == Compression.Lz4)
                    return BuildOptions.CompressWithLz4;
                else if (EditorUserBuildSettings.GetCompressionType(buildTargetGroup) == Compression.Lz4HC)
                    return BuildOptions.CompressWithLz4HC;
            }

            return BuildOptions.None;
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            str.AppendLine("locationPathName = " + BuildPlayerOptions.locationPathName);
            str.AppendLine("target = " + BuildPlayerOptions.target);
            str.AppendLine("scenes = " + string.Join(", ", BuildPlayerOptions.scenes));
            str.AppendLine("assetBundleManifestPath = " + BuildPlayerOptions.assetBundleManifestPath);
            str.AppendLine("options.Development = " + ((BuildPlayerOptions.options & BuildOptions.Development) != 0));
            str.AppendLine("options.AutoRunPlayer = " + ((BuildPlayerOptions.options & BuildOptions.AutoRunPlayer) != 0));
            str.AppendLine("options.ForceEnableAssertions = " + ((BuildPlayerOptions.options & BuildOptions.ForceEnableAssertions) != 0));
            return str.ToString();
        }
    }
}
