using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.GUI.Controls;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    static class TestRunnerUIFilterUtility
    {
        public static ISelectableItem<TestPlatformSelection>[] GetTestPlatformSelectables(out int[] separatorIndices)
        {
            var builtInPlatformSelectables = GetBuiltInTestPlatformSelectables();
            var playerBuilderSelectables = GetPlayerBuilderTestPlatformSelectables();
            var customRunnerSelectables = GetCustomRunnerTestPlatformSelectables();
            var selectables = builtInPlatformSelectables.Concat(playerBuilderSelectables).Concat(customRunnerSelectables);
            var categoryRanges = new[]
            {
                builtInPlatformSelectables.Length - 1,
                builtInPlatformSelectables.Length + playerBuilderSelectables.Length - 1
            };

            separatorIndices = categoryRanges.Distinct().ToArray();
            return selectables.ToArray();
        }

        static ISelectableItem<TestPlatformSelection>[] GetBuiltInTestPlatformSelectables()
        {
            var editorSelectable = new TestPlatformSelection(TestPlatformTarget.Editor);
            var playerSelectable = new TestPlatformSelection(TestPlatformTarget.Player, TestRunnerApi.GetDefaultPlayerBuilderName());
#if UNITY_2021_2_OR_NEWER
            var playerType = (EditorUserBuildSettings.activeBuildTargetGroup == BuildTargetGroup.Standalone &&
                EditorUserBuildSettings.standaloneBuildSubtarget == StandaloneBuildSubtarget.Server)
                ? "Server" : "Player";
#else
            var playerType = "Player";
#endif
            return new ISelectableItem<TestPlatformSelection>[]
            {
                new SelectableItemContent<TestPlatformSelection>(editorSelectable, "In Editor"),
                new SelectableItemContent<TestPlatformSelection>(playerSelectable, $"On {playerType} ({EditorUserBuildSettings.activeBuildTarget})")
            };
        }

        static ISelectableItem<TestPlatformSelection>[] GetPlayerBuilderTestPlatformSelectables()
        {
            var playerBuilderNames = GetCustomPlayerBuilderNames();
            return GetCustomTestPlatformSelectables(TestPlatformTarget.CustomPlayer, playerBuilderNames,
                name => $"{name} ({EditorUserBuildSettings.activeBuildTarget})");
        }

        static ISelectableItem<TestPlatformSelection>[] GetCustomRunnerTestPlatformSelectables()
        {
            var customRunnerNames = TestRunnerApi.GetCustomRunnerNames();
            return GetCustomTestPlatformSelectables(TestPlatformTarget.CustomRunner, customRunnerNames);
        }

        static ISelectableItem<TestPlatformSelection>[] GetCustomTestPlatformSelectables(TestPlatformTarget customPlatform,
            IReadOnlyList<string> customPlatformNames, Func<string, string> displayNameGenerator = null)
        {
            var customPlatformSelectables = new ISelectableItem<TestPlatformSelection>[customPlatformNames.Count];
            for (var i = 0; i < customPlatformNames.Count; i++)
            {
                var customPlatformName = customPlatformNames[i];
                var playerBuilderSelectable = new TestPlatformSelection(customPlatform, customPlatformName);
                var displayName = displayNameGenerator != null ? displayNameGenerator(customPlatformName) : customPlatformName;
                customPlatformSelectables[i] = new SelectableItemContent<TestPlatformSelection>(playerBuilderSelectable, displayName);
            }

            return customPlatformSelectables;
        }

        static string[] GetCustomPlayerBuilderNames()
        {
            try
            {
                var defaultPlayerBuilderName = TestRunnerApi.GetDefaultPlayerBuilderName();
                return TestRunnerApi.GetPlayerBuilderNames()
                    .Where(name => name != defaultPlayerBuilderName)
                    .ToArray();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return new string[0];
            }
        }
    }
}
