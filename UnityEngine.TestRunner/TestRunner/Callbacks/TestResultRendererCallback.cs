using NUnit.Framework.Interfaces;

namespace UnityEngine.TestTools.TestRunner.Callbacks
{
    internal class TestResultRendererCallback : MonoBehaviour, ITestRunnerListener
    {
        private TestResultRenderer m_ResultRenderer;
        public void OnGUI()
        {
            if (m_ResultRenderer != null)
                m_ResultRenderer.Draw();
        }

        public void TestStarted(ITest test)
        {
        }

        public void TestFinished(ITestResult result)
        {
            if (result.Test.Parent != null)
            {
                return;
            }

            if (Camera.main == null)
            {
                gameObject.AddComponent<Camera>();
            }
            m_ResultRenderer = new TestResultRenderer(result);
            m_ResultRenderer.ShowResults();
        }
    }
}
