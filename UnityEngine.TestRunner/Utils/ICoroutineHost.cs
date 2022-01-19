using System.Collections;

namespace UnityEngine.TestRunner.Utils
{
    public interface ICoroutineHost
    {
        Coroutine StartCoroutine(IEnumerator coroutine);
        void StopCoroutine(Coroutine coroutine);
        void StopCoroutine(IEnumerator coroutine);
    }

    public class MonoBehaviourCoroutineHost : ICoroutineHost
    {
        private readonly MonoBehaviour m_MonoBehaviour;

        public MonoBehaviourCoroutineHost(MonoBehaviour monoBehaviour)
        {
            m_MonoBehaviour = monoBehaviour;
        }

        public Coroutine StartCoroutine(IEnumerator coroutine)
        {
            return m_MonoBehaviour.StartCoroutine(coroutine);
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            m_MonoBehaviour.StopCoroutine(coroutine);
        }

        public void StopCoroutine(IEnumerator coroutine)
        {
            m_MonoBehaviour.StopCoroutine(coroutine);
        }
    }
}
