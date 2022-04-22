namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Player
{
    internal interface IPlayerCommunication
    {
        void Init();
        PlayerCommunication.TestStartedEvent OnTestStarted { get; }
        PlayerCommunication.TestFinishedEvent OnTestFinished { get; }
        PlayerCommunication.PlayerAliveHeartbeatEvent OnPlayerAliveHeartbeat { get; }
        void SendQuitMessageAndDisconnect();
    }
}
