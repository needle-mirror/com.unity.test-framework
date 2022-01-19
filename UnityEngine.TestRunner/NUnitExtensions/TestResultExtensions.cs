using System;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEngine.TestTools.TestRunner;

namespace UnityEngine.TestRunner.NUnitExtensions
{
    internal static class TestResultExtensions
    {
        public static void RecordPrefixedException(this TestResult testResult, string prefix, Exception ex, ResultState resultState = null, string additionalMessage = null)
        {
            if (ex is NUnitException)
            {
                ex = ex.InnerException;
            }

            if (resultState == null)
            {
                resultState = testResult.ResultState == ResultState.Cancelled
                    ? ResultState.Cancelled
                    : ResultState.Error;
            }

            var exceptionMessage = ExceptionHelper.BuildMessage(ex);
            string stackTrace = "--" + prefix + NUnit.Env.NewLine + ExceptionHelper.BuildStackTrace(ex);
            if (testResult.StackTrace != null)
            {
                stackTrace = testResult.StackTrace + NUnit.Env.NewLine + stackTrace;
            }

            if (testResult.Test.IsSuite)
            {
                resultState = resultState.WithSite(FailureSite.TearDown);
            }

            if (ex is ResultStateException)
            {
                exceptionMessage = ex.Message;
                resultState = ((ResultStateException)ex).ResultState;
                stackTrace = StackFilter.Filter(ex.StackTrace);
            }

            string message = (string.IsNullOrEmpty(prefix) ? "" : (prefix + " : ")) + exceptionMessage;
            if (testResult.Message != null)
            {
                message = testResult.Message + NUnit.Env.NewLine + message;
            }

            if (additionalMessage != null)
            {
                message += NUnit.Env.NewLine + additionalMessage;
            }

            testResult.SetResult(resultState, message, stackTrace);
        }

        public static void RecordPrefixedExceptionWithHint(this TestResult testResult, string prefix, Exception ex, ResultState resultState = null)
        {
            RecordPrefixedException(testResult, prefix, ex, resultState, GetRelevantHint(ex));
        }

        public static void RecordExceptionWithHint(this TestResult testResult, Exception ex, FailureSite? site = null)
        {
            var hint = GetRelevantHint(ex);
            if (hint == null)
            {
                if (site.HasValue)
                {
                    testResult.RecordException(ex, site.Value);
                }
                else
                {
                    testResult.RecordException(ex);
                }
            }
            else
            {
                testResult.SetResult(site.HasValue ? ResultState.Error.WithSite(site.Value) : ResultState.Error,
                    ExceptionHelper.BuildMessage(ex) + NUnit.Env.NewLine + hint,
                    ExceptionHelper.BuildStackTrace(ex));
            }
        }

        private static string[] PlayModeStrings =
        {
            "can only be used during play mode",
            "can only be used in play mode",
            "not be called from edit mode",
            "EditMode test can only yield null",
            "should be playing",
            "during edit mode"
        };
        private const string RequirePlayModeHint = "Hint: Test is not in PlayMode. If it is intended to be in PlayMode, the [RequiresPlayModeAttribute] can be added to the test, the fixture or the whole assembly.";
        private static string GetRelevantHint(Exception ex)
        {
            var isPlaying = Application.isPlaying;
            var isRelevantException = ex is InvalidOperationException || ex is UnhandledLogMessageException;
            if (isRelevantException && !isPlaying && PlayModeStrings.Any(str => ex.Message.Contains(str)))
            {
                return RequirePlayModeHint;
            }

            return null;
        }

        public static void RecordPrefixedError(this TestResult testResult, string prefix, string error, ResultState resultState = null)
        {
            if (resultState == null)
            {
                resultState = testResult.ResultState == ResultState.Cancelled
                    ? ResultState.Cancelled
                    : ResultState.Error;
            }

            if (testResult.Test.IsSuite)
            {
                resultState = resultState.WithSite(FailureSite.TearDown);
            }

            string message = (string.IsNullOrEmpty(prefix) ? "" : (prefix + " : ")) + error;
            if (testResult.Message != null)
            {
                message = testResult.Message + NUnit.Env.NewLine + message;
            }

            testResult.SetResult(resultState, message);
        }
    }
}
