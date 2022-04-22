using NUnit.Framework.Constraints;

namespace UnityEngine.TestTools.Constraints
{
    public static class ConstraintExtensions
    {
        /// <summary>
        /// Creates a new Assert constraint for asserting on GC memory. See <see cref="AllocatingGCMemoryConstraint"/>.
        /// </summary>
        /// <param name="chain">The constraint expression to be appended to.</param>
        /// <returns>The resulting constraint</returns>
        public static AllocatingGCMemoryConstraint AllocatingGCMemory(this ConstraintExpression chain)
        {
            var constraint = new AllocatingGCMemoryConstraint();
            chain.Append(constraint);
            return constraint;
        }
    }
}
