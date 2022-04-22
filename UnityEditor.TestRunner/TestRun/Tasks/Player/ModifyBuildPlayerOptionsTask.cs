using System;
using System.Collections;
using System.Linq;
using UnityEditor.TestTools;
using UnityEditor.TestTools.TestRunner.TestRun;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks;

namespace TestRun.Tasks.Player
{
    internal class ModifyBuildPlayerOptionsTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.GetReferencedAssemblies().Any(z => z.Name == "UnityEditor.TestRunner")).ToArray();
            var attributes = allAssemblies.SelectMany(assembly => assembly.GetCustomAttributes(typeof(TestPlayerBuildModifierAttribute), true).OfType<TestPlayerBuildModifierAttribute>()).ToArray();
            var modifiers = attributes.Select(attribute => attribute.ConstructModifier()).ToArray();

            foreach (var modifier in modifiers)
            {
                testJobData.PlayerBuildOptions = modifier.ModifyOptions(testJobData.PlayerBuildOptions);
            }

            yield return null;
        }
    }
}
