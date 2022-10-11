using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Tests
{
    internal class ScriptAddingTests
    {
        private const string k_fileName = @"Assets\\Tests\\TempScript.cs"; 
        
        [Test]
        public void YourTestGoesHere()
        {
            
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