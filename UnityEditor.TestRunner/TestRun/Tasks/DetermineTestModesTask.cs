using System.Collections;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Filters;
using UnityEngine.TestRunner.NUnitExtensions.Filters;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class DetermineTestModesTask : TestTaskBase
    {
        public DetermineTestModesTask()
        {
            RerunAfterResume = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            var modesHasAlreadyBeenDetermined = testJobData.hasTestThatRequiresPlayMode ||
                testJobData.hasTestThatDoesNotRequiresPlayMode;

            testJobData.requirePlayModeFilter = new AndFilterExtended(new RequiresPlayModeFilter(true), testJobData.testFilter);
            testJobData.doesNotRequirePlayModeFilter = new AndFilterExtended(new RequiresPlayModeFilter(false), testJobData.testFilter);

            if (modesHasAlreadyBeenDetermined)
            {
                yield break;
            }

            testJobData.hasTestThatRequiresPlayMode = FilterMatchesAnyTest(testJobData.requirePlayModeFilter, testJobData.testTree);
            yield return null;
            testJobData.hasTestThatDoesNotRequiresPlayMode = FilterMatchesAnyTest(testJobData.doesNotRequirePlayModeFilter, testJobData.testTree);

            if (!testJobData.hasTestThatRequiresPlayMode && !testJobData.hasTestThatDoesNotRequiresPlayMode)
            {
                testJobData.hasTestThatDoesNotRequiresPlayMode = true;
            }
            yield return null;
        }

        private static bool FilterMatchesAnyTest(ITestFilter filter, ITest testTree)
        {
            var passFilter = filter.Pass(testTree);
            if (!passFilter || !testTree.IsSuite)
            {
                return passFilter;
            }

            return testTree.Tests.Any(test => FilterMatchesAnyTest(filter, test));
        }
    }
}
