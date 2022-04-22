using System;
using System.Collections;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks.Player;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class PlayerHeartBeatTask : TestTaskBase
    {
        internal IPlayerCommunication playerCommunication = PlayerCommunication.instance;
        internal Func<double> getTime = () => EditorApplication.timeSinceStartup;

        public override string GetName()
        {
            return "Receiving test data from player";
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            var timeout = testJobData.executionSettings.playerHeartbeatTimeout;
            var lastHeartbeatReceivedTime = getTime();

            testJobData.RunStartedEvent.AddListener((t) =>
            {
                lastHeartbeatReceivedTime = getTime();
            });
            testJobData.RunFinishedEvent.AddListener((t) =>
            {
                lastHeartbeatReceivedTime = getTime();
            });
            testJobData.TestStartedEvent.AddListener((t) => { lastHeartbeatReceivedTime = getTime(); });
            testJobData.TestFinishedEvent.AddListener((t) => { lastHeartbeatReceivedTime = getTime(); });
            playerCommunication.OnPlayerAliveHeartbeat.AddListener(() => { lastHeartbeatReceivedTime = getTime(); });

            while (!testJobData.PlayerHasFinished)
            {
                if (getTime() - lastHeartbeatReceivedTime > timeout)
                {
                    throw new Exception($"Test execution timed out. No activity received from the player in {timeout} seconds.");
                }

                yield return null;
            }
        }
    }
}
