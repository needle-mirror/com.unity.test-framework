using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework.Interfaces;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestRunner.TestLaunchers;

namespace UnityEngine.TestTools.TestRunner.Callbacks
{
    [AddComponentMenu("")]
    internal class RemoteTestResultSender : MonoBehaviour, ITestRunnerListener
    {
        public bool ReportBackToEditor = true;
        private class QueueData
        {
            public Guid id { get; set; }
            public byte[] data { get; set; }
        }

        private const int k_aliveMessageFrequency = 120;
        private float m_NextliveMessage = k_aliveMessageFrequency;

        public void Start()
        {
            PlayerConnection.instance.Register(PlayerConnectionMessageIds.quitPlayerMessageId, ProcessPlayerQuitMessage);
        }

        private byte[] SerializeObject(object objectToSerialize)
        {
            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(objectToSerialize));
        }

        public void TestStarted(ITest test)
        {
            if (!ReportBackToEditor)
            {
                return;
            }
            Send(PlayerConnectionMessageIds.testStartedMessageId, Encoding.UTF8.GetBytes(test.GetUniqueName()));
        }

        public void TestFinished(ITestResult result)
        {
            if (!ReportBackToEditor)
            {
                return;
            }

            var resultData = SerializeObject(TestResultSerializer.MakeFromTestResult(result));
            Send(PlayerConnectionMessageIds.testFinishedMessageId, resultData);
        }

        private void Send(Guid messageId, byte[] data)
        {
            PlayerConnection.instance.Send(messageId, data);
            ResetNextPlayerAliveMessageTime();
        }

        public IEnumerator SendDataRoutine()
        {
            while (!PlayerConnection.instance.isConnected)
            {
                yield return new WaitForSeconds(1);
            }

            while (true)
            {
                SendAliveMessageIfNeeded();
                yield return new WaitForSeconds(k_aliveMessageFrequency);
            }
        }

        private void ProcessPlayerQuitMessage(MessageEventArgs arg0)
        {
            //Some platforms don't quit, so we need to disconnect to make sure they will not connect to another editor instance automatically.
            PlayerConnection.instance.DisconnectAll();

            // The XboxOne platform is being removed, and is not shipped as of Unity 2021.1.
            // When 2020.3 LTS stops being released support for the XboxOne platform can be removed, it was replaced by GameCoreXboxOne from 2019.4 onwards.
#if !UNITY_2021_1_OR_NEWER
            //XBOX has an error when quitting
            if (Application.platform == RuntimePlatform.XboxOne)
            {
                return;
            }
#endif
            Application.Quit();
        }

        private void SendAliveMessageIfNeeded()
        {
            if (Time.timeSinceLevelLoad < m_NextliveMessage)
            {
                return;
            }
            Debug.Log("Sending player alive message back to editor.");
            Send(PlayerConnectionMessageIds.playerAliveHeartbeat, new byte[0]);
        }

        private void ResetNextPlayerAliveMessageTime()
        {
            m_NextliveMessage = Time.timeSinceLevelLoad + k_aliveMessageFrequency;
        }
    }
}
