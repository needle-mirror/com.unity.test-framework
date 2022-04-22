using System;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    [Flags]
    enum RequirePlaymodeMode
    {
        TestsNotRequiringPlaymodeInEditor = 1 << 0,
        TestsRequiringPlaymodeInEditor = 1 << 1
    }
}
