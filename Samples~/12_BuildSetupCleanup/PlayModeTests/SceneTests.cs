﻿using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class SceneTests
    {
        private string originalScene;
        private const string k_SceneName = "Assets/MyGameScene.unity";
        
        [UnitySetUp]
        public IEnumerator SetupBeforeTest()
        {
            originalScene = SceneManager.GetActiveScene().path;
            SceneManager.LoadScene(k_SceneName);
            yield return null; // Skip a frame, allowing the scene to load.
        }

        [Test]
        public void VerifyScene()
        {
            var gameObject = GameObject.Find("GameObjectToTestFor");

            Assert.That(gameObject, Is.Not.Null, $"GameObjectToTestFor not found in {SceneManager.GetActiveScene().path}.");
        }

        [TearDown]
        public void TeardownAfterTest()
        {
            SceneManager.LoadScene(originalScene);
        }
    }
}
