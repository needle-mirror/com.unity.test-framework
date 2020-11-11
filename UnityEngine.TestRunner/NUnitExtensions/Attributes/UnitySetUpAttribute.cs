using System;

namespace UnityEngine.TestTools
{
  [AttributeUsage(AttributeTargets.Method)]
  public class UnitySetUpAttribute: NUnitAttribute
  {
  }

  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
  public class UnityOneTimeSetUpAttribute: NUnitAttribute
  {
  }
}
