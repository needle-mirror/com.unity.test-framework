using System;
using System.Collections;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks.Player;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class QuitPlayerTask : TestTaskBase
    {
        internal IPlayerCommunication playerCommunication = PlayerCommunication.instance;

        public QuitPlayerTask()
        {
            RunOnCancel = true;
            RunOnError = ErrorRunMode.RunAlways;
            CanRunInstantly = false;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            playerCommunication.SendQuitMessageAndDisconnect();
            // Give the player a moment to disconnect
            var waitUntil = DateTime.UtcNow.AddSeconds(1);
            while (waitUntil > DateTime.UtcNow)
            {
                yield return null;
            }
        }
    }
}
