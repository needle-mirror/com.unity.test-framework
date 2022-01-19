using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UnityEngine.TestTools.TestRunner.Callbacks
{
    [AddComponentMenu("")]
    internal class PlayModeRunnerCallback : MonoBehaviour, ITestRunnerListener
    {
        private TestResultRenderer m_ResultRenderer;

        public void TestFinished(ITestResult result)
        {
            if (result.Test.Parent != null)
            {
                return;
            }

            Application.logMessageReceivedThreaded -= LogRecieved;
            if (Camera.main == null)
            {
                gameObject.AddComponent<Camera>();
            }

            m_ResultRenderer = new TestResultRenderer(result);
            m_ResultRenderer.ShowResults();
        }

        public void OnGUI()
        {
            if (m_ResultRenderer != null)
                m_ResultRenderer.Draw();
        }

        public void TestStarted(ITest test)
        {
            if (test.Parent == null)
            {
                Application.logMessageReceivedThreaded += LogRecieved;
            }
        }

        private void LogRecieved(string message, string stacktrace, LogType type)
        {
            if (TestContext.Out != null)
                TestContext.Out.WriteLine(message);
        }
    }
}
