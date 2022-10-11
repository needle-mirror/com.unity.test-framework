using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SkippingFrameTests
    {

        private GameObject PrepareCube()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 1;
            rb.velocity = Vector3.down * 5;
            return go;
        }
        
        [UnityTest]
        public IEnumerator CubeMovesDown()
        {
            // todo: Create the test body
            Assert.Fail("TODO");
            yield break;
        }
    }
}
