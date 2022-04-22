using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.TestTools.Logging;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal abstract class BuildActionTaskBase<T> : TestTaskBase
    {
        private string typeName;
        private Func<TestJobData, ITestFilter> filterSelector;
        internal IAttributeFinder attributeFinder;
        internal Action<string> logAction = Debug.Log;
        internal Func<ILogScope> logScopeProvider = () => new LogScope();
        internal Func<Type, object> createInstance = Activator.CreateInstance;

        protected BuildActionTaskBase(Func<TestJobData, ITestFilter> filterSelector, IAttributeFinder attributeFinder)
        {
            this.filterSelector = filterSelector;
            this.attributeFinder = attributeFinder;
            typeName = typeof(T).Name;
        }

        protected abstract void Action(T target);

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.testTree == null)
            {
                throw new Exception($"Test tree is not available for {GetType().Name}.");
            }

            // This flag is used by older versions of the graphics test framework in order to determine if the pre-build setup is for a player (false) or for the editor (true).
            PlaymodeLauncher.IsRunning = !testJobData.executionSettings.PlayerIncluded();
            var enumerator = ExecuteMethods(testJobData.testTree, filterSelector(testJobData), testJobData.TargetRuntimePlatform ?? Application.platform);

            while (enumerator.MoveNext())
            {
                yield return null;
            }
        }

        private IEnumerator ExecuteMethods(ITest testTree, ITestFilter testRunnerFilter, RuntimePlatform targetPlatform)
        {
            var exceptions = new List<Exception>();

            foreach (var targetClassType in attributeFinder.Search(testTree, testRunnerFilter, targetPlatform))
            {
                try
                {
                    var targetClass = (T)createInstance(targetClassType);

                    logAction($"Executing {typeName} for: {targetClassType.FullName}.");

                    using (var logScope = logScopeProvider())
                    {
                        Action(targetClass);

                        if (logScope.AnyFailingLogs())
                        {
                            var failingLog = logScope.FailingLogs.First();
                            throw new UnhandledLogMessageException(failingLog);
                        }

                        if (logScope.ExpectedLogs.Any())
                        {
                            var expectedLogs = logScope.ExpectedLogs.First();
                            throw new UnexpectedLogMessageException(expectedLogs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

                yield return null;
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException($"One or more exceptions when executing {typeName}.", exceptions);
            }
        }
    }
}
