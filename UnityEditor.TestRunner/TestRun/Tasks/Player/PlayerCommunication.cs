using System;
using System.Collections.Generic;
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

        enum MessageType
        {
            TestStarted,
            TestFinished,
        }

        [Serializable]
        struct Message
        {
            public byte[] Bytes;
            public MessageType Type;
        }

        [SerializeField]
        private List<Message> m_IncomingMessages = new List<Message>();

        [SerializeField]
        private bool m_RegisteredMessageCallback;

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
            // When a message comes in, we should not immediately process it but instead enqueue it for processing later
            // in the frame. The problem this solves is that Unity only reserves about 1ms worth of time every frame to
            // process message from the player connection. When some tests run in a player, it can take the editor
            // minutes to react to all messages we receive because we only do 1ms of processing, then render all of the
            // editor etc. -- Instead, we use that 1ms time-window to enqueue messages and then react to them later
            // during the frame. This reduces the waiting time from minutes to seconds.
            EditorConnection.instance.Register(PlayerConnectionMessageIds.testStartedMessageId, args => EnqueueMessage(args, MessageType.TestStarted));
            EditorConnection.instance.Register(PlayerConnectionMessageIds.testFinishedMessageId, args => EnqueueMessage(args, MessageType.TestFinished));
            EditorConnection.instance.Register(PlayerConnectionMessageIds.playerAliveHeartbeat, PlayerAliveHeartbeat);
        }

        private void FlushMessageQueue()
        {
            EditorApplication.update -= FlushMessageQueue;
            m_RegisteredMessageCallback = false;
            foreach (var msg in m_IncomingMessages)
            {
                switch (msg.Type)
                {
                    case MessageType.TestFinished:
                    {
                        var result = JsonUtility.FromJson<TestResultSerializer>(Encoding.UTF8.GetString(msg.Bytes));
                        OnTestFinished.Invoke(result);
                        break;
                    }
                    case MessageType.TestStarted:
                    {
                        var fullName = Encoding.UTF8.GetString(msg.Bytes);
                        OnTestStarted.Invoke(fullName);
                        break;
                    }
                }
            }
            m_IncomingMessages.Clear();
        }

        private void EnqueueMessage(MessageEventArgs args, MessageType type)
        {
            m_ActivePlayerId = args.playerId;
            if (!m_RegisteredMessageCallback)
            {
                EditorApplication.update += FlushMessageQueue;
                m_RegisteredMessageCallback = true;
            }
            m_IncomingMessages.Add(new Message
            {
                Bytes = args.data,
                Type = type
            });
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
