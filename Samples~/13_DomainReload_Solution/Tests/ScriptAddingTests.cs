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
        private const string k_fileName = @"Assets\\Tests\\TempScript.cs"; 
        
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
            if (!File.Exists(k_fileName))
            {
                yield break;
            }
            
            File.Delete(k_fileName);
            yield return new RecompileScripts();
        }
        
        private void CreateScript()
        {
            File.WriteAllText(k_fileName, @"
            public class MyTempScript {
                public string Verify()
                {
                    return ""OK"";
                }    
            }");
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