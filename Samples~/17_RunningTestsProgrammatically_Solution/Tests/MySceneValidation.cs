using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    [TestFixture]
    [Category("SceneValidation")]
    public class MySceneValidation
    {
        private string k_SceneName = @"Assets\MyScene.unity";

        [OneTimeSetUp]
        public void LoadScene()
        {
            EditorSceneManager.OpenScene(k_SceneName);
        }
        
        [Test]
        public void MySceneContainsFiveMyMonoBehaviors()
        {
            var myMonoBehaviors = Object.FindObjectsOfType<MyMonoBehaviour>();
            
            Assert.That(myMonoBehaviors.Length, Is.EqualTo(5), $"Incorrect number of {nameof(myMonoBehaviors)} in scene.");
        }

        [Test]
        public void AllMyMonobehaviorsAreConfigured()
        {
            var myMonoBehaviors = Object.FindObjectsOfType<MyMonoBehaviour>();

            var monobehaviorsConfigured = myMonoBehaviors.Where(behavior => behavior.IsConfigured).ToArray();
            
            Assert.That(monobehaviorsConfigured.Length, Is.EqualTo(myMonoBehaviors.Length), $"No all {nameof(myMonoBehaviors)} where configured.");
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }
        
    }
}
