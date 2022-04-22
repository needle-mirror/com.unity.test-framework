using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;
using System;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// This attribute is an alternative to the standard `Ignore` attribute in NUnit. It allows for ignoring tests based on arguments which were passed to the test method.
    /// </summary>
    /// <example>
    /// The following example shows a method to use the `ParameterizedIgnore` attribute to ignore only one test with specific combination of arguments, where `someString` is `b` and `someInt` is `10`.
    /// <code>
    /// <![CDATA[
    /// using NUnit.Framework;
    /// using System.Collections.Generic;
    /// using UnityEngine.TestTools;
    ///
    /// public class MyTestsClass
    /// {
    ///    public static IEnumerable<TestCaseData> MyTestCaseSource()
    ///    {
    ///        for (int i = 0; i < 5; i++)
    ///        {
    ///            yield return new TestCaseData("a", i);
    ///            yield return new TestCaseData("b", i);
    ///        }
    ///    }
    ///
    ///    [Test, TestCaseSource(nameof(MyTestCaseSource))]
    ///    [ParameterizedIgnore("b", 3)]
    ///    public void Test(string someString, int someInt)
    ///    {
    ///        Assert.Pass();
    ///    }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    /// It could also be used together with the `Values` attribute in NUnit:
    /// <code>
    /// <![CDATA[
    /// using NUnit.Framework;
    /// using UnityEngine.TestTools;
    ///
    /// public class MyTestsClass
    /// {
    ///    [Test]
    ///    [ParameterizedIgnore("b", 10)]
    ///    public void Test(
    ///        [Values("a", "b")] string someString,
    ///        [Values(5, 10)] int someInt)
    ///    {
    ///        Assert.Pass();
    ///    }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ParameterizedIgnoreAttribute : NUnitAttribute, IWrapTestMethod
    {
        public object[] Arguments { get; }
        public string Reason { get; set; }

        public ParameterizedIgnoreAttribute(params object[] Arguments)
        {
            this.Arguments = Arguments;
        }

        public TestCommand Wrap(TestCommand command)
        {
            return new ParameterizedIgnoreCommand(command, Arguments, Reason);
        }
    }
}
