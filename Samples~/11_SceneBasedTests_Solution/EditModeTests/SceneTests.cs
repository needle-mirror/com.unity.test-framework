using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SceneTests
    {
        [SetUp]
        public void Setup()
        {
            EditorSceneManager.OpenScene("Assets/MyGameScene.unity");
        }
        
        [Test]
        public void VerifyScene()
        {
            var gameObject = GameObject.Find("GameObjectToTestFor");
            
            Assert.That(gameObject, Is.Not.Null);
        }

        [TearDown]
        public void Teardown()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }
    }
}
