using System.Collections;
using UnityEditor.SceneManagement;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Scene
{
    internal class SaveSceneTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            EditorSceneManager.SaveScene(testJobData.InitTestScene, testJobData.InitTestScenePath, false);
            yield return null;
        }
    }
}
