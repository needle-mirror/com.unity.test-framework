using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEngine.TestRunner.NUnitExtensions.Filters;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// Use this attribute to instruct the test framework to run tests in PlayMode or EditMode when run in the Editor, independent of the assembly configuration.
    /// The attribute can be applied to a single test method, a test fixture, or the whole assembly.
    /// If specified on multiple levels, the test follows the attribute on the lowest level. E.g. if the assembly has `[RequiresPlayMode(false)]`, which runs all tests in EditMode, and the test fixture has `[RequiresPlayMode]`, which runs all tests in PlayMode, then a test in that fixture will run in PlayMode, taking the configuration from the fixture.
    /// The attribute allows for two new combinations:
    ///  * Editor-only tests that always run in PlayMode.
    ///  * Tests that can run on platforms, but that run without entering PlayMode when run in the Editor.
    ///
    /// By default, any Editor-only assembly has the equivalent of `[RequiresPlayMode(false)]` applied, while an assembly with platform support has the equivalent of `[RequiresPlayMode]` applied.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiresPlayModeAttribute : NUnitAttribute, IApplyToTest
    {
        internal const string k_RequiresPlayModeString = "RequiresPlayMode";
        private bool m_RequiresPlayMode;

        /// <summary>
        /// Flags whether a test, test fixture, or assembly should run in PlayMode when run in the Editor.
        /// </summary>
        /// <param name="requiresPlayMode">If true, the test is always run in PlayMode. If false, the test is always run in EditMode.</param>
        public RequiresPlayModeAttribute(bool requiresPlayMode = true)
        {
            m_RequiresPlayMode = requiresPlayMode;
        }

        /// <summary>
        /// Modifies a test as defined for the specific attribute.
        /// </summary>
        /// <param name="test">The test to modify</param>
        public void ApplyToTest(Test test)
        {
            test.Properties.Set(k_RequiresPlayModeString, m_RequiresPlayMode);
        }

        internal static bool? GetValueForTest(ITest test)
        {
            return test.Properties.ContainsKey(k_RequiresPlayModeString)
                ? test.Properties.Get(k_RequiresPlayModeString) as bool?
                : null;
        }
    }
}
