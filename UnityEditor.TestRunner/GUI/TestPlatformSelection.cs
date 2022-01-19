using System;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    [Serializable]
    struct TestPlatformSelection : IEquatable<TestPlatformSelection>
    {
        [SerializeField]
        string m_CustomTargetName;
        public string CustomTargetName => m_CustomTargetName ?? string.Empty;

        [field: SerializeField]
        public TestPlatformTarget PlatformTarget { get; private set; }

        public TestPlatformSelection(TestPlatformTarget platformTarget, string customTargetName = default)
        {
            PlatformTarget = platformTarget;
            m_CustomTargetName = customTargetName;
        }

        public bool Equals(TestPlatformSelection other)
        {
            return PlatformTarget == other.PlatformTarget && CustomTargetName == other.CustomTargetName;
        }

        public override bool Equals(object obj)
        {
            return obj is TestPlatformSelection other && Equals(other);
        }

        public static bool operator==(TestPlatformSelection left, TestPlatformSelection right)
        {
            return left.Equals(right);
        }

        public static bool operator!=(TestPlatformSelection left, TestPlatformSelection right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)PlatformTarget * 397) ^ CustomTargetName.GetHashCode();
            }
        }
    }
}
