using System.Linq;
using UnityEditor.TestTools.TestRunner.Api;

namespace UnityEditor.TestTools.TestRunner
{
    internal static class PlaymodeLauncher
    {
        // This flag is used by older versions of the graphics test framework in order to determine if the pre-build setup is for a player (false) or for the editor (true).
        public static bool IsRunning = true;
    }
}
