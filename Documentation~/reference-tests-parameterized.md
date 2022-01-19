# Parameterized tests

For data-driven testing, you may want to have your tests parameterized. You can use both NUnit [TestCaseAttribute](https://docs.nunit.org/articles/nunit/writing-tests/attributes/testcase.html) and [ValueSourceAttribute](https://docs.nunit.org/articles/nunit/writing-tests/attributes/valuesource.html) with a unit test. 

> **Note**: With a `UnityTest` only `ValueSource` is supported.  

## Example

```c#
static int[] values = new int[] { 1, 5, 6 };

[UnityTest]
public IEnumerator MyTestWithMultipleValues([ValueSource("values")] int value)
{
    yield return null;
}
```
## Ignore based on parameters

You can selectively ignore tests based on the parameters supplied to the test method by using the [ParameterizedIgnoreAttribute](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/api/UnityEngine.TestTools.ParameterizedIgnoreAttribute.html).
