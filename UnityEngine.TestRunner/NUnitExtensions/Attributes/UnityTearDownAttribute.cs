using System;
using NUnit.Framework;

namespace UnityEngine.TestTools
{
  [AttributeUsage(AttributeTargets.Method)]
  public class UnityTearDownAttribute: NUnitAttribute
  {
  }

  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
  public class UnityOneTimeTearDownAttribute: NUnitAttribute
  {
  }
}
