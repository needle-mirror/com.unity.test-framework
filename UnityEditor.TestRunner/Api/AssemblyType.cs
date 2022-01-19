using System;

namespace UnityEditor.TestTools.TestRunner.Api
{
    /// <summary>
    /// Enum flags indicating if an assembly is editor only or if the assembly supports both editor and platforms.
    /// </summary>
    [Flags]
    [Serializable]
    public enum AssemblyType
    {
        /// <summary>
        /// The assembly is editor only and is not available on any platforms.
        /// </summary>
        EditorOnly = 1 << 0,
        /// <summary>
        /// The assembly is available in the editor and on one or more platforms.
        /// </summary>
        EditorAndPlatforms = 1 << 1
    }
}
