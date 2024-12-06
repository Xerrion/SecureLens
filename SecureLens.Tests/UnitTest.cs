using Xunit;

namespace SecureLens.Tests
{
    public class UnitTest
    {
        [Fact]
        public void TestContainsSecure_ShouldReturnTrue_WhenInputIsSecure()
        {
            // Arrange
            var input = "This is secure!";

            // Act
            var result = SecureLens.StringOperations.ContainsSecure(input);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void TestContainsSecure_ShouldReturnFalse_WhenInputIsNotSecure()
        {
            // Arrange
            var input = "This is not safe.";

            // Act
            var result = SecureLens.StringOperations.ContainsSecure(input);

            // Assert
            Assert.False(result);
        }
    }
}