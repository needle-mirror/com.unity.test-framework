using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEngine.Profiling;

namespace UnityEngine.TestTools.Constraints
{
    /// <summary>
    /// An NUnit test constraint class to test whether a given block of code makes any GC allocations.
    ///
    /// Use this class with NUnit's Assert.That() method to make assertions about the GC behaviour of your code. The constraint executes the delegate you provide, and checks if it has caused any GC memory to be allocated. If any GC memory was allocated, the constraint passes; otherwise, the constraint fails.
    ///
    /// Be careful to take into account that there may be GC allocations that happen only on the first time your code is run, such as initialization of static variables. If you want to measure a 'typical' execution of your code, you may want to deliberately execute it once before testing it with this constraint, so that any one-time allocations have already been performed before this constraint measures it.
    ///
    /// Usually you negate this constraint to make sure that your delegate does not allocate any GC memory. This is easy to do using the Is class:
    /// </summary>
    /// <example>
    /// <code>
    /// using NUnit.Framework;
    /// using UnityEngine.TestTools.Constraints;
    /// using Is = UnityEngine.TestTools.Constraints.Is;
    ///
    /// public class MyTestClass
    /// {
    ///     [Test]
    ///     public void SettingAVariableDoesNotAllocate()
    ///     {
    ///         Assert.That(() => {
    ///             int a = 0;
    ///             a = 1;
    ///         }, Is.Not.AllocatingGCMemory());
    ///     }
    /// }
    /// </code>
    /// </example>
    public class AllocatingGCMemoryConstraint : Constraint
    {
        internal class GCMeasurementResult
        {
            public int SampleBlockCount { get; }
            public GCMeasurementResult(int sampleBlockCount)
            {
                SampleBlockCount = sampleBlockCount;
            }

            public override string ToString() => $"{SampleBlockCount} GC allocation{(SampleBlockCount == 1 ? "" : "s")}";
        }

        private class AllocatingGCMemoryResult : ConstraintResult
        {
            public AllocatingGCMemoryResult(IConstraint constraint, GCMeasurementResult result) : base(constraint, result, result.SampleBlockCount > 0)
            {
            }

            public override void WriteMessageTo(MessageWriter writer)
            {
                GCMeasurementResult result = (GCMeasurementResult)ActualValue;
                if (result.SampleBlockCount == 0)
                    writer.WriteMessageLine("The provided delegate did not make any GC allocations.");
                else
                    writer.WriteMessageLine("The provided delegate made {0} GC allocation(s).", result.SampleBlockCount);
            }
        }

        private ConstraintResult ApplyTo(Action action, object original)
        {
            // Force JIT compilation of the supplied delegate up-front so that it can't contribute to the GC allocations.
            // There's no way to guarantee that this will cover all of the 'one-time' allocations made by the code - the
            // only way to do that is to execute it, and we can't safely assume that the user has written the code in a
            // way that allows for that - but it will help with some common cases.
            System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(action);

            var recorder = Recorder.Get("GC.Alloc");

            // The recorder was created enabled, which means it captured the creation of the Recorder object itself, etc.
            // Disabling it flushes its data, so that we can retrieve the sample block count and have it correctly account
            // for these initial allocations.
            recorder.enabled = false;

#if !UNITY_WEBGL
            recorder.FilterToCurrentThread();
#endif

            recorder.enabled = true;

            try
            {
                action();
            }
            finally
            {
                recorder.enabled = false;
#if !UNITY_WEBGL
                recorder.CollectFromAllThreads();
#endif
            }


            return new AllocatingGCMemoryResult(this, new GCMeasurementResult(recorder.sampleBlockCount));
        }

        /// <summary>
        /// Applies GC memory constraint to the test.
        /// </summary>
        /// <param name="obj">An object to apply the GC constraint to. Should be a <see cref="TestDelegate"/>.</param>
        /// <returns>A ConstraintResult</returns>
        /// <exception cref="ArgumentNullException">Throws a <see cref="ArgumentNullException"/> if the provided object is null.</exception>
        /// <exception cref="ArgumentException">Throws a <see cref="ArgumentException"/> if the provided object is not a <see cref="TestDelegate"/>.</exception>
        public override ConstraintResult ApplyTo(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();

            TestDelegate d = obj as TestDelegate;
            if (d == null)
                throw new ArgumentException(string.Format("The actual value must be a TestDelegate but was {0}",
                    obj.GetType()));

            // Force JIT
            System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(d);

            return ApplyTo(() => d.Invoke(), obj);
        }

        /// <summary>
        /// Test whether the constraint is satisfied by a given reference.
        /// The default implementation simply dereferences the value but
        /// derived classes may override it to provide for delayed processing.
        /// </summary>
        /// <typeparam name="TActual">The type of the actual value delegate to be tested.</typeparam>
        /// <param name="del">A reference to the value delegate to be tested</param>
        /// <returns>A ConstraintResult</returns>
        /// <exception cref="ArgumentNullException">Throws a <see cref="ArgumentNullException"/> if the provided delegate is null.</exception>
        public override ConstraintResult ApplyTo<TActual>(ActualValueDelegate<TActual> del)
        {
            if (del == null)
                throw new ArgumentNullException();

            // Force JIT
            System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(del);

            return ApplyTo(() => del.Invoke(), del);
        }

        /// <summary>
        /// The Description of what this constraint tests, for to use in messages and in the ConstraintResult.
        /// </summary>
        public override string Description
        {
            get { return "allocates GC memory"; }
        }
    }
}
