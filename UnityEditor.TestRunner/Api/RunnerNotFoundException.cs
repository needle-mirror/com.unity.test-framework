using System;

namespace UnityEditor.TestTools.TestRunner.Api
{
    /// <summary>
    /// Exception for if a runner with a given guid could not be found.
    /// </summary>
    public class RunnerNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the RunnerNotFoundException class.
        /// </summary>
        /// <param name="guid">The guid of the runner that could not be found.</param>
        internal RunnerNotFoundException(string guid) : base($"Could not find runner with id {guid}.")
        {
        }
    }
}
