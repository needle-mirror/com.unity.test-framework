using System;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEngine.TestTools;

namespace UnityEngine.TestRunner.NUnitExtensions.Filters
{
    internal class RequiresPlayModeFilter : NonExplicitFilter
    {
        private readonly bool m_RequirePlayMode;
        internal Func<ITest, bool?> GetValueForTest = RequiresPlayModeAttribute.GetValueForTest;

        public RequiresPlayModeFilter(bool requirePlayMode)
        {
            m_RequirePlayMode = requirePlayMode;
        }

        public override bool Match(ITest test)
        {
            return MatchesValue(GetValueForTest(test));
        }

        private bool MatchesValue(bool? requiresPlayMode)
        {
            return requiresPlayMode.HasValue && requiresPlayMode.Value == m_RequirePlayMode;
        }

        public override bool Pass(ITest test)
        {
            if (MatchDescendant(test))
            {
                return true;
            }

            return MatchesValue(GetFirstExplicitAncestorPlayModeValue(test));
        }

        public override bool IsExplicitMatch(ITest test)
        {
            return false;
        }

        public override TNode AddToXml(TNode parentNode, bool recursive)
        {
            return parentNode.AddElement(RequiresPlayModeAttribute.k_RequiresPlayModeString, m_RequirePlayMode.ToString());
        }

        private bool? GetFirstExplicitAncestorPlayModeValue(ITest test)
        {
            var requiresPlayMode = GetValueForTest(test);
            if (requiresPlayMode.HasValue)
            {
                return requiresPlayMode.Value;
            }

            if (test.Parent != null)
            {
                return GetFirstExplicitAncestorPlayModeValue(test.Parent);
            }

            return null;
        }
    }
}
