using System;

namespace UnityEngine.TestRunner.TestLaunchers
{
    internal static class PlayerConnectionMessageIds
    {
        public static Guid testStartedMessageId { get { return new Guid("bdc866a8-b55b-43e2-83cd-3ee1f63d791f"); } }
        public static Guid testFinishedMessageId { get { return new Guid("939f1fc6-627e-47e4-a9c2-13bdeb2c52f9"); } }
        public static Guid quitPlayerMessageId { get { return new Guid("ab44bfe0-bb50-4ee6-9977-69d2ea6bb3a0"); } }
        public static Guid playerAliveHeartbeat { get { return new Guid("8c0c307b-f7fd-4216-8623-35b4b3f55fb6"); } }
    }
}
