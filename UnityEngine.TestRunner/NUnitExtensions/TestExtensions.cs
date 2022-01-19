using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEngine.TestRunner.NUnitExtensions.Filters;

namespace UnityEngine.TestRunner.NUnitExtensions
{
    internal static class TestExtensions
    {
        private static List<string> ExtractFixtureCategories(ITest test)
        {
            var fixtureCategories = test.Properties[PropertyNames.Category].Cast<string>().ToList();
            if (test.Parent != null)
            {
                fixtureCategories.AddRange(ExtractFixtureCategories(test.Parent));
            }

            return fixtureCategories;
        }

        public static List<string> GetAllCategoriesFromTest(this ITest test)
        {
            // Only mark tests as Uncategorized if the test fixture doesn't have a category,
            // otherwise the test inherits the Fixture category.
            // Recursively try checking until Parent is null - cause category can be set on higher level.
            var categories = ExtractFixtureCategories(test);
            if (categories.Count == 0 && test is TestMethod)
            {
                categories.Add(CategoryFilterExtended.k_DefaultCategory);
            }
            return categories;
        }

        public static void ParseForNameDuplicates(this ITest test, Dictionary<string, int> duplicates = null)
        {
            if (duplicates == null)
            {
                duplicates = new Dictionary<string, int>();
            }

            for (var i = 0; i < test.Tests.Count; i++)
            {
                var child = test.Tests[i];
                var uniqueName = GetUniqueName(child);
                int count;
                if (duplicates.TryGetValue(uniqueName, out count))
                {
                    count++;
                    child.Properties.Add("childIndex", count);
                    duplicates[uniqueName] = count;
                }
                else
                {
                    duplicates.Add(uniqueName, 1);
                }


                // Use the duplicates dictionary across all levels to ensure that identical tests with different parents still are unique.
                // E.g. a test inside a parameterized fixture where the fixture has a non unique name.
                ParseForNameDuplicates(child, duplicates);
            }
        }

        public static int GetChildIndex(this ITest test)
        {
            var index = test.Properties["childIndex"];
            return (int)index[0];
        }

        public static bool HasChildIndex(this ITest test)
        {
            return test.Properties.ContainsKey("childIndex") && test.Properties["childIndex"].Count > 0;
        }

        private static string GetAncestorPath(ITest test)
        {
            var path = "";
            var testParent = test.Parent;

            while (testParent != null && testParent.Parent != null && !string.IsNullOrEmpty(testParent.Name))
            {
                path = testParent.Name + "/" + path;
                testParent = testParent.Parent;
            }

            return path;
        }

        public static string GetUniqueName(this ITest test)
        {
            var id = string.Empty;
            if (test.GetType() == typeof(TestSuite))
            {
                // This ensures that suites on the namespace level includes the dll name.
                id += GetAncestorPath(test);
            }
            else if (test.IsSuite && test.TypeInfo != null) // Test class / fixture
            {
                id += GetAssemblyName(test) + "/";
            }

            id += GetFullNameWithoutDllPath(test);
            if (test.HasChildIndex())
            {
                var index = test.GetChildIndex();
                if (index >= 0)
                    id += index;
            }
            if (test.IsSuite)
            {
                id += "[suite]";
            }
            return id;
        }

        public static string GetFullNameWithoutDllPath(this ITest test)
        {
            if (test.Properties != null)
            {
                if (test.Properties.ContainsKey("isRoot"))
                {
                    return test.Name;
                }

                if (test.Properties.ContainsKey("isAssembly"))
                {
                    return test.Name;
                }
            }

            return test.FullName;
        }

        private static string GetAssemblyName(ITest test)
        {
            while (true)
            {
                if (test.Properties.ContainsKey("isAssembly"))
                {
                    return test.Name;
                }

                if (test.Parent == null)
                {
                    return string.Empty;
                }

                test = test.Parent;
            }
        }

        public static string GetSkipReason(this ITest test)
        {
            if (test.Properties.ContainsKey(PropertyNames.SkipReason))
                return (string)test.Properties.Get(PropertyNames.SkipReason);

            return null;
        }

        public static string GetParentId(this ITest test)
        {
            if (test.Parent != null)
                return test.Parent.Id;

            return null;
        }

        public static string GetParentFullName(this ITest test)
        {
            if (test.Parent != null)
                return test.Parent.FullName;

            return null;
        }

        public static string GetParentUniqueName(this ITest test)
        {
            if (test.Parent != null)
                return GetUniqueName(test.Parent);

            return null;
        }
    }
}
