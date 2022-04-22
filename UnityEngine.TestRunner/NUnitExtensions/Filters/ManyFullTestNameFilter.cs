using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;

namespace UnityEngine.TestRunner.NUnitExtensions.Filters
{
    internal class ManyFullTestNameFilter : NUnit.Framework.Internal.Filters.ValueMatchFilter
    {
        private readonly HashSet<string> m_TestNames;
        public ManyFullTestNameFilter(IEnumerable<string> testNames) : base("")
        {
            m_TestNames = new HashSet<string>(testNames);
        }

        public override bool Match(ITest test)
        {
            return m_TestNames.Contains(test.GetFullNameWithoutDllPath());
        }

        internal bool IsTestFor(string name) => m_TestNames.Contains(name);

        public override TNode AddToXml(TNode parentNode, bool recursive) => throw new NotImplementedException();

        protected override string ElementName => "test";
    }
}
