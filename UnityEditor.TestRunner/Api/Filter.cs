using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.TestRunner.GUI;
#pragma warning disable 618

namespace UnityEditor.TestTools.TestRunner.Api
{
    /// <summary>
    /// The filter class provides the <see cref="TestRunnerApi"/> with a specification of what tests to run when [running tests programmatically](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/extension-run-tests.html).
    /// </summary>
    [Serializable]
    public class Filter : ISerializationCallbackReceiver
    {
        /// <summary>
        /// An enum flag that specifies if Edit Mode or Play Mode tests should run.
        /// </summary>
        [SerializeField]
        public TestMode testMode;

        /// <summary>
        /// A nullable boolean flag specifying where to include tests that requires PlayMode into the filter. If left as null, tests are included regardless of if they require PlayMode or not.
        /// </summary>
        [NonSerialized]
        public bool? requiresPlayMode;

        [SerializeField]
        private bool m_RequiresPlayModeValue;

        [SerializeField]
        private bool m_RequiresPlayModeIsSet;

        /// <summary>
        /// An enum flag that filters whether the assembly of the test is in an editor only assembly or if it is in an assembly that also supports one or more platforms.
        /// </summary>
        [SerializeField]
        public AssemblyType assemblyType = AssemblyType.EditorOnly | AssemblyType.EditorAndPlatforms;
        /// <summary>
        /// The full name of the tests to match the filter. This is usually in the format FixtureName.TestName. If the test has test arguments, then include them in parenthesis. E.g. MyTestClass2.MyTestWithMultipleValues(1).
        /// </summary>
        [SerializeField]
        public string[] testNames;
        /// <summary>
        /// The same as testNames, except that it allows for Regex. This is useful for running specific fixtures or namespaces. E.g. "^MyNamespace\\." Runs any tests where the top namespace is MyNamespace.
        /// </summary>
        [SerializeField]
        public string[] groupNames;
        /// <summary>
        /// The name of a [Category](https://nunit.org/docs/2.2.7/category.html) to include in the run. Any test or fixtures runs that have a Category matching the string.
        /// </summary>
        [SerializeField]
        public string[] categoryNames;
        /// <summary>
        /// The name of assemblies included in the run. That is the assembly name, without the .dll file extension. E.g., MyTestAssembly
        /// </summary>
        [SerializeField]
        public string[] assemblyNames;
        /// <summary>
        /// The <see cref="BuildTarget"/> platform to run the test on. If set to null, then the Editor is the target for the tests.
        /// </summary>
        public BuildTarget? targetPlatform;

        [SerializeField]
        private BuildTarget m_TargetPlatform;

        public void OnBeforeSerialize()
        {
            m_TargetPlatform = targetPlatform ?? BuildTarget.NoTarget;
            if (requiresPlayMode.HasValue)
            {
                m_RequiresPlayModeIsSet = true;
                m_RequiresPlayModeValue = requiresPlayMode.Value;
            }
            else
            {
                m_RequiresPlayModeIsSet = false;
                m_RequiresPlayModeValue = false;
            }
        }

        public void OnAfterDeserialize()
        {
            targetPlatform = m_TargetPlatform == BuildTarget.NoTarget ? null : (BuildTarget?)m_TargetPlatform;
            requiresPlayMode = m_RequiresPlayModeIsSet ? (bool?)m_RequiresPlayModeValue : null;
        }

        /// <summary>
        /// Implementation of ToString() that builds a string composed of the filter values.
        /// </summary>
        /// <returns>The current filter values as a string.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(Filter)} with settings:");
            stringBuilder.AppendLine($"{nameof(testMode)} = {testMode}");
            stringBuilder.AppendLine($"{nameof(requiresPlayMode)} = {requiresPlayMode}");
            stringBuilder.AppendLine($"{nameof(assemblyType)} = {assemblyType}");
            stringBuilder.AppendLine($"{nameof(targetPlatform)} = {targetPlatform}");
            stringBuilder.AppendLine($"{nameof(testNames)} = " + (testNames == null ? "null" : string.Join(", ", testNames)));
            stringBuilder.AppendLine($"{nameof(groupNames)} = " + (groupNames == null ? "null" : string.Join(", ", groupNames)));
            stringBuilder.AppendLine($"{nameof(categoryNames)} = " + (categoryNames == null ? "null" : string.Join(", ", categoryNames)));
            stringBuilder.AppendLine($"{nameof(assemblyNames)} = " + (assemblyNames == null ? "null" : string.Join(", ", assemblyNames)));

            return stringBuilder.ToString();
        }

        internal RuntimeTestRunnerFilter ToRuntimeTestRunnerFilter(bool synchronousOnly)
        {
            return new RuntimeTestRunnerFilter()
            {
                testMode = ConvertTestMode(testMode),
                hasRequiresPlayModeFlag = requiresPlayMode.HasValue,
                requiresPlayMode = requiresPlayMode != null && requiresPlayMode.Value,
                testNames = testNames,
                categoryNames = categoryNames,
                groupNames = groupNames,
                assemblyNames = assemblyNames,
                synchronousOnly = synchronousOnly,
                filterEditorOnly = assemblyType == AssemblyType.EditorOnly,
                filterEditorAndPlatformsOnly = assemblyType == AssemblyType.EditorAndPlatforms
            };
        }

        private static TestPlatform ConvertTestMode(TestMode testMode)
        {
            if (testMode == (TestMode.EditMode | TestMode.PlayMode))
            {
                return TestPlatform.All;
            }

            if (testMode == TestMode.EditMode)
            {
                return TestPlatform.EditMode;
            }

            if (testMode == TestMode.PlayMode)
            {
                return TestPlatform.PlayMode;
            }

            return 0;
        }

        /// <summary>
        /// Implementation of Equals() that compares two objects to determine if they are equal.
        /// </summary>
        /// <param name="obj">An object to compare with this filter.</param>
        /// <returns>True if the supplied object is the same as this filter.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Filter);
        }

        /// <summary>
        /// Implementation of Equals() that compares two filters to determine if they are equal.
        /// </summary>
        /// <param name="p">A filter to compare with this filter.</param>
        /// <returns>True if the supplied filter is reference equal to this filter.</returns>
        public bool Equals(Filter p)
        {
            if (ReferenceEquals(p, null))
                return false;

            if (ReferenceEquals(this, p))
                return true;

            if (GetType() != p.GetType())
                return false;

            return testMode == p.testMode &&
                requiresPlayMode == p.requiresPlayMode &&
                targetPlatform == p.targetPlatform &&
                testNames.SequenceEqual(p.testNames) &&
                groupNames.SequenceEqual(p.groupNames) &&
                categoryNames.SequenceEqual(p.categoryNames) &&
                assemblyNames.SequenceEqual(p.assemblyNames);
        }

        /// <summary>
        /// Implementation of GetHashCode() that computes a hash code from the current filter values.
        /// </summary>
        /// <returns>A hash code for this filter.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + testMode.GetHashCode();
                hash = (hash * 7) + assemblyType.GetHashCode();
                if (requiresPlayMode != null)
                    hash = (hash * 7) + requiresPlayMode.GetHashCode();
                if (targetPlatform != null)
                    hash = (hash * 7) + targetPlatform.GetHashCode();
                if (testNames != null)
                    hash = (hash * 7) + testNames.GetHashCode();
                if (groupNames != null)
                    hash = (hash * 7) + groupNames.GetHashCode();
                if (categoryNames != null)
                    hash = (hash * 7) + categoryNames.GetHashCode();
                if (assemblyNames != null)
                    hash = (hash * 7) + assemblyNames.GetHashCode();
                return hash;
            }
        }

        internal bool HasAny()
        {
            return requiresPlayMode.HasValue ||
                assemblyNames != null && assemblyNames.Any()
                || categoryNames != null && categoryNames.Any()
                || groupNames != null && groupNames.Any()
                || testNames != null && testNames.Any();
        }
    }
}
