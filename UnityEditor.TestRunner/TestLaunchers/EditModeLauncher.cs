using System;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.GUI;
using UnityEngine;
using UnityEngine.TestRunner.Utils;
using UnityEngine.TestTools;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner
{
    internal class EditModeLauncher : TestLauncherBase
    {
        internal readonly EditModeRunner m_EditModeRunner;
        public bool launchedOutsideApi;

        // provided for backward compatibility with Rider UnitTesting prior to Rider package v.1.1.1
        public EditModeLauncher(UITestRunnerFilter filter, TestPlatform platform)
        {
            launchedOutsideApi = true;
            var apiFilter = new[]
            {
                new Filter
                {
                    testMode = TestMode.EditMode,
                    testNames = filter.testNames,
                    categoryNames = filter.categoryNames,
                    groupNames = filter.groupNames,
                    assemblyNames = filter.assemblyNames
                }
            };
            
            ScriptableObject.CreateInstance<TestRunnerApi>().Execute(new ExecutionSettings(apiFilter));
        }

        public EditModeLauncher(Filter[] filters, TestPlatform platform, bool runSynchronously,
            RunStartedEvent runStartedEvent, TestStartedEvent testStartedEvent, TestFinishedEvent testFinishedEvent, RunFinishedEvent runFinishedEvent,
            string[] orderedTestNames)
        {
            TestEnumerator.Reset();
            m_EditModeRunner = ScriptableObject.CreateInstance<EditModeRunner>();
            m_EditModeRunner.UnityTestAssemblyRunnerFactory = new UnityTestAssemblyRunnerFactory();
            m_EditModeRunner.Init(filters, platform, runSynchronously,runStartedEvent, testStartedEvent, testFinishedEvent, runFinishedEvent, orderedTestNames);
        }

        public override void Run()
        {
            if (launchedOutsideApi)
            {
                // Do not use the launcher, as it will be relaunched trough the api. See ctor.
                return;
            }

            var callback = AddEventHandler<EditModeRunnerCallback>();
            callback.runner = m_EditModeRunner;
            AddEventHandler<CallbacksDelegatorListener>();

            m_EditModeRunner.Run();
            
            if (m_EditModeRunner.RunningSynchronously)
                m_EditModeRunner.CompleteSynchronously();
        }

        public T AddEventHandler<T>() where T : ScriptableObject, ITestRunnerListener
        {
            return m_EditModeRunner.AddEventHandler<T>();
        }
    }
}
