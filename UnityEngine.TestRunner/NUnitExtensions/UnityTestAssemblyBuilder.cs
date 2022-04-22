using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using Unity.Profiling;
using UnityEngine.TestRunner.NUnitExtensions.Filters;

namespace UnityEngine.TestTools.NUnitExtensions
{
    internal class UnityTestAssemblyBuilder : DefaultTestAssemblyBuilder, IAsyncTestAssemblyBuilder
    {
        private readonly string m_ProductName;
        public UnityTestAssemblyBuilder()
        {
            m_ProductName = Application.productName;
        }

        public UnityTestAssemblyBuilder(string productName)
        {
            m_ProductName = productName;
        }

        public ITest Build(AssemblyWithPlatform[] assemblies)
        {
            var test = BuildAsync(assemblies);
            while (test.MoveNext())
            {
            }

            return test.Current;
        }

        struct PlatformAssembly : IEquatable<PlatformAssembly>
        {
            public System.Reflection.Assembly Assembly;
            public TestPlatform Platform;

            public bool Equals(PlatformAssembly other)
            {
                return Equals(Assembly, other.Assembly) && Platform == other.Platform;
            }

            public override bool Equals(object obj)
            {
                return obj is PlatformAssembly other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Assembly != null ? Assembly.GetHashCode() : 0) * 397) ^ (int)Platform;
                }
            }
        }

        private static Dictionary<PlatformAssembly, TestSuite> CachedAssemblies = new Dictionary<PlatformAssembly, TestSuite>();
        public IEnumerator<ITest> BuildAsync(AssemblyWithPlatform[] assemblies)
        {
            var productName = string.Join("_", m_ProductName.Split(Path.GetInvalidFileNameChars()));
            var suite = new TestSuite(productName);
            suite.Properties.Set("isRoot", true);
            for (var index = 0; index < assemblies.Length; index++)
            {
                var assembly = assemblies[index].AssemblyWrapper.Assembly;
                var platform = assemblies[index].TestPlatform;

                using (new ProfilerMarker(nameof(UnityTestAssemblyBuilder) + "." + assembly.GetName().Name).Auto())
                {
                    var key = new PlatformAssembly {Assembly = assembly, Platform = platform};
                    if (!CachedAssemblies.TryGetValue(key, out var assemblySuite))
                    {
                        assemblySuite = Build(assembly, GetNUnitTestBuilderSettings(platform)) as TestSuite;
                        if (assemblySuite != null)
                        {
                            assemblySuite.Properties.Set("platform", platform);
                            assemblySuite.Properties.Set("isAssembly", true);
                            EditorOnlyFilter.ApplyPropertyToTest(assemblySuite, platform == TestPlatform.EditMode);

                            if (RequiresPlayModeAttribute.GetValueForTest(assemblySuite) == null)
                            {
                                new RequiresPlayModeAttribute(platform == TestPlatform.PlayMode).ApplyToTest(assemblySuite);
                            }
                        }
                        CachedAssemblies.Add(key, assemblySuite);
                    }

                    if (assemblySuite != null && assemblySuite.HasChildren)
                    {
                        suite.Add(assemblySuite);
                    }
                }

                yield return null;
            }

            yield return suite;
        }

        public static Dictionary<string, object> GetNUnitTestBuilderSettings(TestPlatform testPlatform)
        {
            var emptySettings = new Dictionary<string, object>();
            emptySettings.Add(FrameworkPackageSettings.TestParameters, "platform=" + testPlatform);
            return emptySettings;
        }
    }
}
