using MyExercise;
using NUnit.Framework;

namespace Tests
{
    internal class StringFormatterTests
    {
        [Test]
        public void JoinsObjectsWithSemiColon()
        {
            // Arrange
            var formatterUnderTest = new StringFormatter();
            formatterUnderTest.Configure(";");
            var objects = new object[] {"a", "bc", 5, "d"};
            
            // Act
            var result = formatterUnderTest.Join(objects);
            
            // Assert
            Assert.AreEqual("a;bc;5;d", result);
        }
    }
}