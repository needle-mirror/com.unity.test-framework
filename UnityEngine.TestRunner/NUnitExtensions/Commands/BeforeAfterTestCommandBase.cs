using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.Logging;
using UnityEngine.TestTools.TestRunner;

namespace UnityEngine.TestTools
{
    internal abstract class BeforeAfterTestCommandBase<T> : DelegatingTestCommand, IEnumerableTestMethodCommand where T : class
    {
        private string m_BeforeErrorPrefix;
        private string m_AfterErrorPrefix;
        private bool m_SkipYieldAfterActions;
        protected BeforeAfterTestCommandBase(TestCommand innerCommand, string beforeErrorPrefix, string afterErrorPrefix, bool skipYieldAfterActions = false)
            : base(innerCommand)
        {
            m_BeforeErrorPrefix = beforeErrorPrefix;
            m_AfterErrorPrefix = afterErrorPrefix;
            m_SkipYieldAfterActions = skipYieldAfterActions;
        }

        internal Func<long> GetUtcNow = () => new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

        protected T[] BeforeActions = new T[0];

        protected T[] AfterActions = new T[0];

        protected static MethodInfo[] GetActions(IDictionary<Type, List<MethodInfo>> cacheStorage, Type fixtureType, Type attributeType, Type[] returnTypes)
        {
            if (cacheStorage.TryGetValue(fixtureType, out var result))
            {
                return result.ToArray();
            }

            cacheStorage[fixtureType] = GetMethodsWithAttributeFromFixture(fixtureType,  attributeType, returnTypes);

            return cacheStorage[fixtureType].ToArray();
        }

        protected static T[] GetTestActions(IDictionary<MethodInfo,  List<T>> cacheStorage, MethodInfo methodInfo)
        {
            if (cacheStorage.TryGetValue(methodInfo, out var result))
            {
                return result.ToArray();
            }

            var attributesForMethodInfo = new List<T>();
            var attributes = methodInfo.GetCustomAttributes(false);
            foreach (var attribute in attributes)
            {
                if (attribute is T attribute1)
                {
                    attributesForMethodInfo.Add(attribute1);
                }
            }

            cacheStorage[methodInfo] = attributesForMethodInfo;

            return cacheStorage[methodInfo].ToArray();
        }

        private static List<MethodInfo> GetMethodsWithAttributeFromFixture(Type fixtureType, Type setUpType, Type[] returnTypes)
        {
            MethodInfo[] methodsWithAttribute = Reflect.GetMethodsWithAttribute(fixtureType, setUpType, true);
            var methodsInfo = new List<MethodInfo>();
            methodsInfo.AddRange(methodsWithAttribute.Where(method => returnTypes.Any(type => type == method.ReturnType)));
            return methodsInfo;
        }

        protected abstract IEnumerator InvokeBefore(T action, Test test, UnityTestExecutionContext context);

        protected abstract IEnumerator InvokeAfter(T action, Test test, UnityTestExecutionContext context);

        protected virtual bool MoveBeforeEnumerator(IEnumerator enumerator, Test test)
        {
            return enumerator.MoveNext();
        }

        protected virtual bool MoveAfterEnumerator(IEnumerator enumerator, Test test)
        {
            return enumerator.MoveNext();
        }

        protected abstract BeforeAfterTestCommandState GetState(UnityTestExecutionContext context);

        public IEnumerable ExecuteEnumerable(ITestExecutionContext context)
        {
            var unityContext = (UnityTestExecutionContext)context;
            var state = GetState(unityContext);

            // When entering PlayMode state will incorrectly be seen as null. Looking at the hashcode to be certain that it is null.
            if (state?.GetHashCode() == null)
            {
                // We do not expect a state to exist in playmode
                state = new BeforeAfterTestCommandState();
            }

            state.ApplyTestResult(context.CurrentResult);

            while (state.NextBeforeStepIndex < BeforeActions.Length)
            {
                state.Timestamp = GetUtcNow();
                var action = BeforeActions[state.NextBeforeStepIndex];
                IEnumerator enumerator;
                try
                {
                    enumerator = InvokeBefore(action, Test, unityContext);
                }
                catch (Exception ex)
                {
                    state.TestHasRun = true;
                    context.CurrentResult.RecordPrefixedExceptionWithHint(m_BeforeErrorPrefix, ex);
                    break;
                }
                ActivePcHelper.SetEnumeratorPC(enumerator, state.NextBeforeStepPc);

                using (var logScope = new LogScope())
                {
                    while (true)
                    {
                        try
                        {
                            if (!enumerator.MoveNext())
                            {
                                logScope.EvaluateLogScope(true);
                                break;
                            }

                            if (m_SkipYieldAfterActions) // Evaluate the log scope right away for the commands where we do not yield
                            {
                                logScope.EvaluateLogScope(true);
                            }
                        }
                        catch (Exception ex)
                        {
                            state.TestHasRun = true;
                            context.CurrentResult.RecordPrefixedExceptionWithHint(m_BeforeErrorPrefix, ex);
                            state.StoreTestResult(context.CurrentResult);
                            break;
                        }

                        state.NextBeforeStepPc = ActivePcHelper.GetEnumeratorPC(enumerator);
                        state.StoreTestResult(context.CurrentResult);
                        if (m_SkipYieldAfterActions)
                        {
                            break;
                        }
                        else
                        {
                            yield return enumerator.Current;
                        }

                        if (GetUtcNow() - state.Timestamp > unityContext.TestCaseTimeout)
                        {
                            context.CurrentResult.RecordPrefixedExceptionWithHint(m_BeforeErrorPrefix, new UnityTestTimeoutException(unityContext.TestCaseTimeout));
                            state.TestHasRun = true;
                            break;
                        }
                        if (context.CurrentResult.ResultState.Status == TestStatus.Failed)
                        {
                            // In case the coroutine runner or any top level item has failed
                            state.TestHasRun = true;
                            break;
                        }
                    }
                }

                state.NextBeforeStepIndex++;
                state.NextBeforeStepPc = 0;
            }

            if (!state.TestHasRun)
            {
                if (innerCommand is IEnumerableTestMethodCommand)
                {
                    var executeEnumerable = ((IEnumerableTestMethodCommand)innerCommand).ExecuteEnumerable(context);
                    foreach (var iterator in executeEnumerable)
                    {
                        state.StoreTestResult(context.CurrentResult);
                        yield return iterator;
                    }
                }
                else
                {
                    context.CurrentResult = innerCommand.Execute(context);
                    state.StoreTestResult(context.CurrentResult);
                }

                state.TestHasRun = true;
            }

            while (state.NextAfterStepIndex < AfterActions.Length)
            {
                state.Timestamp = GetUtcNow();
                state.TestAfterStarted = true;
                var action = AfterActions[state.NextAfterStepIndex];
                IEnumerator enumerator;
                try
                {
                    enumerator = InvokeAfter(action, Test, unityContext);
                }
                catch (Exception ex)
                {
                    context.CurrentResult.RecordPrefixedExceptionWithHint(m_AfterErrorPrefix, ex);
                    state.StoreTestResult(context.CurrentResult);
                    break;
                }
                ActivePcHelper.SetEnumeratorPC(enumerator, state.NextAfterStepPc);

                using (var logScope = new LogScope())
                {
                    while (true)
                    {
                        try
                        {
                            if (!enumerator.MoveNext())
                            {
                                logScope.EvaluateLogScope(true);
                                break;
                            }

                            if (m_SkipYieldAfterActions) // Evaluate the log scope right away for the commands where we do not yield
                            {
                                logScope.EvaluateLogScope(true);
                            }
                        }
                        catch (Exception ex)
                        {
                            context.CurrentResult.RecordPrefixedExceptionWithHint(m_AfterErrorPrefix, ex);
                            state.StoreTestResult(context.CurrentResult);
                            break;
                        }

                        state.NextAfterStepPc = ActivePcHelper.GetEnumeratorPC(enumerator);
                        state.StoreTestResult(context.CurrentResult);
                        if (GetUtcNow() - state.Timestamp > unityContext.TestCaseTimeout)
                        {
                            context.CurrentResult.RecordPrefixedExceptionWithHint(m_AfterErrorPrefix, new UnityTestTimeoutException(unityContext.TestCaseTimeout));
                            yield break;
                        }

                        if (m_SkipYieldAfterActions)
                        {
                            break;
                        }
                        else
                        {
                            yield return enumerator.Current;
                        }
                    }
                }

                state.NextAfterStepIndex++;
                state.NextAfterStepPc = 0;
            }

            state.Reset();
        }

        public override TestResult Execute(ITestExecutionContext context)
        {
            throw new NotImplementedException("Use ExecuteEnumerable");
        }

        private static TestCommandPcHelper pcHelper;

        internal static TestCommandPcHelper ActivePcHelper
        {
            get
            {
                if (pcHelper == null)
                {
                    pcHelper = new TestCommandPcHelper();
                }

                return pcHelper;
            }
            set
            {
                pcHelper = value;
            }
        }
    }
}
