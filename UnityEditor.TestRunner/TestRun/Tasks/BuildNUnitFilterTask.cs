using System.Collections;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Filters;
using UnityEngine.TestRunner.NUnitExtensions.Filters;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class BuildNUnitFilterTask : TestTaskBase
    {
        public BuildNUnitFilterTask()
        {
            RerunAfterResume = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            var executionSettings = testJobData.executionSettings;
            ITestFilter filter = new OrFilter(executionSettings.filters.Select(f => f.ToRuntimeTestRunnerFilter(executionSettings.runSynchronously).BuildNUnitFilter()).ToArray());

            if (executionSettings.PlayerIncluded())
            {
                filter = new AndFilterExtended(new EditorOnlyFilter(false), filter);
            }

            testJobData.testFilter = filter;

            yield return null;
        }
    }
}
