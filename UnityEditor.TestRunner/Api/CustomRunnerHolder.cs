using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.TestTools.TestRunner.Api
{
    class CustomRunnerHolder : ScriptableSingleton<CustomRunnerHolder>, ICustomRunnerHolder
    {
        List<CustomRunnerBase> m_CustomRunners = new List<CustomRunnerBase>();

        public void Add(CustomRunnerBase customRunner)
        {
            m_CustomRunners.Add(customRunner);
        }

        public void Remove(CustomRunnerBase customRunner)
        {
            m_CustomRunners.Remove(customRunner);
        }

        public CustomRunnerBase[] GetAll()
        {
            return m_CustomRunners.ToArray();
        }

        public CustomRunnerBase Get(string customRunnerName)
        {
            return m_CustomRunners.FirstOrDefault(r =>
                r.name.Equals(customRunnerName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
