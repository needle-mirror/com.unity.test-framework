using System;

namespace UnityEditor.TestTools.TestRunner.Api
{
    interface ICustomRunnerHolder
    {
        void Add(CustomRunnerBase customRunner);
        void Remove(CustomRunnerBase customRunner);
        CustomRunnerBase[] GetAll();
        CustomRunnerBase Get(string name);
    }
}
