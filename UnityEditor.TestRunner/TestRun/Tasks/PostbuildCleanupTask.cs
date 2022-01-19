using System;
using System.Collections;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class PostbuildCleanupTask : BuildActionTaskBase<IPostBuildCleanup>
    {
        public PostbuildCleanupTask(Func<TestJobData, ITestFilter> filterSelector) : base(filterSelector, new PostbuildCleanupAttributeFinder())
        {
            RunOnError = ErrorRunMode.RunAlways;
        }

        protected override void Action(IPostBuildCleanup target)
        {
            target.Cleanup();
        }
    }
}
