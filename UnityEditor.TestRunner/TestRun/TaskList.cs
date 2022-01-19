using System.Collections.Generic;
using TestRun.Tasks.Player;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks.Events;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks.Platform;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks.Player;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks.Scene;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    internal static class TaskList
    {
        public static IEnumerable<TestTaskBase> GetTaskList(ExecutionSettings settings)
        {
            if (settings == null)
            {
                yield break;
            }

            if (!string.IsNullOrEmpty(settings.customRunnerName))
            {
                yield return new CustomRunnerTask();
                yield break;
            }

            if (settings.PlayerIncluded())
            {
                yield return new SaveModifiedSceneTask();
                yield return new StoreSceneSetupTask();
                yield return new CreateBootstrapSceneTask(true, true, NewSceneSetup.EmptyScene);
                yield return new DetermineRuntimePlatformTask();
                yield return new PlatformSpecificSetupTask();
                yield return new BuildNUnitFilterTask();
                yield return new BuildTestTreeTask();
                yield return new CreateEventsTask();
                yield return new PlayerEventDelegatorTask();
                yield return new RegisterCallbackDelegatorEventsTask();
                yield return new SetupProjectParametersTask();
                yield return new PreparePlayerSceneTask();
                yield return new PrebuildSetupTask(data => data.testFilter);
                yield return new SaveSceneTask();
                yield return new SetupBuildPlayerOptionsTask();
                yield return new ModifyBuildPlayerOptionsTask();
                yield return new ConditionalTask(new RegisterPlayerRunFinishTask(), data => !data.executionSettings.IsBuildOnly);
                yield return new BuildAndRunPlayerTask();
                yield return new PlatformSpecificSuccessfulBuildTask();
                yield return new PlatformSpecificSuccessfulLaunchTask();
                yield return new PostbuildCleanupTask(data => data.testFilter);
                yield return new ConditionalTask(new PlayerHeartBeatTask(), data => !data.executionSettings.IsBuildOnly);
                yield return new ConditionalTask(new QuitPlayerTask(), data => !data.executionSettings.IsBuildOnly);
                yield return new CleanupProjectParametersTask();
                yield return new PlatformSpecificCleanupTask();
                yield return new RestoreSceneSetupTask();
                yield return new DeleteBootstrapSceneTask();
                yield break;
            }

            yield return new JobStageTask("Setup");
            yield return new SaveModifiedSceneTask();
            yield return new RegisterFilesForCleanupVerificationTask();
            yield return new SaveUndoIndexTask();
            yield return new StoreSceneSetupTask();
            yield return new RemoveAdditionalUntitledSceneTask();
            yield return new ReloadModifiedScenesTask();
            yield return new BuildNUnitFilterTask();
            yield return new BuildTestTreeTask();
            yield return new DetermineTestModesTask();
            yield return new ConditionalTask(new CreateBootstrapSceneTask(false, false, NewSceneSetup.DefaultGameObjects), data => data.hasTestThatDoesNotRequiresPlayMode);
            yield return new CreateEventsTask();
            yield return new RegisterCallbackDelegatorEventsTask();
            yield return new RegisterTestRunCallbackEventsTask();
            yield return new EnableTestOutLoggerTask();
            yield return new InitializeTestProgressTask();
            yield return new UpdateTestProgressTask();
            yield return new GenerateContextTask();
            yield return new SetupConstructDelegatorTask();
            yield return new TestResultSerializerTask();

            // Non PlayMode only
            yield return new ConditionalTask(new JobStageTask("Running tests"), data => data.hasTestThatDoesNotRequiresPlayMode);
            yield return new ConditionalTask(new PrebuildSetupTask(data => data.doesNotRequirePlayModeFilter), data => data.hasTestThatDoesNotRequiresPlayMode);
            yield return new ConditionalTask(new RunStartedInvocationEvent(), data => data.hasTestThatDoesNotRequiresPlayMode);
            yield return new ConditionalTask(new EditModeRunTask(), data => data.hasTestThatDoesNotRequiresPlayMode);
            yield return new ConditionalTask(new PostbuildCleanupTask(data => data.doesNotRequirePlayModeFilter), data => data.hasTestThatDoesNotRequiresPlayMode);

            // PlayMode only
            yield return new ConditionalTask(new CreateBootstrapSceneTask(false, true, NewSceneSetup.EmptyScene), data => data.hasTestThatRequiresPlayMode);
            yield return new ConditionalTask(new JobStageTask("Preparing PlayMode"), data => data.hasTestThatRequiresPlayMode && data.hasTestThatDoesNotRequiresPlayMode);
            yield return new ConditionalTask(new PrebuildSetupTask(data => data.requirePlayModeFilter), data => data.hasTestThatRequiresPlayMode);
            yield return new ConditionalTask(new PreparePlayModeRunTask(), data => data.hasTestThatRequiresPlayMode);
            yield return new ConditionalTask(new UnlockReloadAssembliesTask(), data => data.hasTestThatDoesNotRequiresPlayMode && data.hasTestThatRequiresPlayMode);
            yield return new ConditionalTask(new EnterPlayModeTask(), data => data.hasTestThatRequiresPlayMode);
            yield return new ConditionalTask(new JobStageTask("Running tests - In PlayMode"), data => data.hasTestThatRequiresPlayMode);
            yield return new ConditionalTask(new RunStartedInvocationEvent(), data => data.hasTestThatRequiresPlayMode && !data.hasTestThatDoesNotRequiresPlayMode);
            yield return new ConditionalTask(new PlayModeRunTask(), data => data.hasTestThatRequiresPlayMode);
            yield return new ConditionalTask(new PostbuildCleanupTask(data => data.requirePlayModeFilter), data => data.hasTestThatRequiresPlayMode);
            yield return new RunFinishedInvocationEvent();

            yield return new JobStageTask("Cleanup");
            yield return new ConditionalTask(new CleanupTestControllerTask(), data => data.hasTestThatRequiresPlayMode);
            yield return new ConditionalTask(new ExitPlayModeTask(), data => data.hasTestThatRequiresPlayMode);
            yield return new ConditionalTask(new RestoreProjectSettingsTask(), data => data.hasTestThatRequiresPlayMode);
            yield return new CleanUpContext();
            yield return new CleanupConstructDelegatorTask();
            yield return new RestoreSceneSetupTask();
            yield return new DeleteBootstrapSceneTask();
            yield return new PerformUndoTask();
            yield return new CleanupVerificationTask();
            yield return new UnlockReloadAssembliesTask();
        }
    }
}
