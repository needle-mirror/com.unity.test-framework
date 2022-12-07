using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests
{
    internal class ScriptAddingTests
    {
        private const string pathToFile = "Assets/Tests/TempScript.cs"; 
        
        [UnityTest]
        public IEnumerator CreatedScriptIsVerified()
        {
            CreateScript();
            yield return new RecompileScripts();

            var verification = VerifyScript();
            
            Assert.That(verification, Is.EqualTo("OK"));
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            if (!File.Exists(pathToFile))
            {
                yield break;
            }
            
            File.Delete(pathToFile);
            yield return new RecompileScripts();
        }
        
        private void CreateScript()
        {
            try
            {
                File.WriteAllText(pathToFile, @"
                public class MyTempScript {
                    public string Verify()
                    {
                        return ""OK"";
                    }    
                }");
            }
            catch(DirectoryNotFoundException)
            {
                Assert.Inconclusive("The path to file is incorrect. Please make sure that the path to TempScript is valid.");
            }
        }

        private string VerifyScript()
        {
            Type type = Type.GetType("MyTempScript", true);
            
            object instance = Activator.CreateInstance(type);

            var verifyMethod = type.GetMethod("Verify", BindingFlags.Instance | BindingFlags.Public);

            var verifyResult = verifyMethod.Invoke(instance, new object[0]);
            return verifyResult as string;
        }
    }
}