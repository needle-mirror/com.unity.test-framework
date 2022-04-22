using System;
using System.Text;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.TestRunner.TestLaunchers;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Player
{
    [Serializable]
    internal class PlayerCommunication : ScriptableSingleton<PlayerCommunication>, IPlayerCommunication
    {
        [SerializeField]
        private bool m_RegisteredConnectionCallbacks;

        [SerializeField]
        private int m_ActivePlayerId;

        public TestStartedEvent OnTestStarted { get; } = new TestStartedEvent();
        public TestFinishedEvent OnTestFinished { get; } = new TestFinishedEvent();
        public PlayerAliveHeartbeatEvent OnPlayerAliveHeartbeat { get; } = new PlayerAliveHeartbeatEvent();

        /// <summary>
        /// Initializes the <see cref="EditorConnection"/> and registers callbacks if needed.
        /// </summary>
        public void Init()
        {
            EditorConnection.instance.Initialize();
            if (!m_RegisteredConnectionCallbacks)
            {
                EditorConnection.instance.Initialize();
                DelegateEditorConnectionEvents();
            }
        }

        private void DelegateEditorConnectionEvents()
        {
            m_RegisteredConnectionCallbacks = true;
            EditorConnection.instance.Register(PlayerConnectionMessageIds.testStartedMessageId, TestStarted);
            EditorConnection.instance.Register(PlayerConnectionMessageIds.testFinishedMessageId, TestFinished);
            EditorConnection.instance.Register(PlayerConnectionMessageIds.playerAliveHeartbeat, PlayerAliveHeartbeat);
        }

        private void TestStarted(MessageEventArgs args)
        {
            m_ActivePlayerId = args.playerId;
            var fullName = Encoding.UTF8.GetString(args.data);
            OnTestStarted.Invoke(fullName);
        }

        private void TestFinished(MessageEventArgs args)
        {
            m_ActivePlayerId = args.playerId;
            var result = JsonUtility.FromJson<TestResultSerializer>(Encoding.UTF8.GetString(args.data));
            OnTestFinished.Invoke(result);
        }

        private void PlayerAliveHeartbeat(MessageEventArgs args)
        {
            m_ActivePlayerId = args.playerId;
            OnPlayerAliveHeartbeat.Invoke();
        }

        public void SendQuitMessageAndDisconnect()
        {
            EditorConnection.instance.Send(PlayerConnectionMessageIds.quitPlayerMessageId, null, m_ActivePlayerId);
            EditorConnection.instance.DisconnectAll();
        }

        public class TestStartedEvent : UnityEvent<string> {}
        public class TestFinishedEvent : UnityEvent<TestResultSerializer> {}
        public class PlayerAliveHeartbeatEvent : UnityEvent {}
    }
}
