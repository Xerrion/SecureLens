using Xunit;
using Moq;
using System.IO;
using SecureLens.Infrastructure.Logging;
using SecureLens.Utilities;
using System.Collections.Generic;
using SecureLens.Core.Models;
using SecureLens.Infrastructure.Utilities;

namespace SecureLens.Tests.Utilities
{
    public class JsonHelperTests
    {
        [Fact]
        public void LoadJsonFile_ValidJson_ReturnsDeserializedObject()
        {
            // Arrange
            string filePath = "test_valid.json";
            var expectedData = new List<InventoryLogEntry>
            {
                new InventoryLogEntry { /* Initialize properties */ },
                new InventoryLogEntry { /* Initialize properties */ }
            };
            string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(expectedData);
            File.WriteAllText(filePath, jsonContent);
            var loggerMock = new Mock<ILogger>();

            // Act
            var result = JsonHelper.LoadJsonFile<List<InventoryLogEntry>>(filePath, loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public void LoadJsonFile_FileDoesNotExist_ReturnsDefault()
        {
            // Arrange
            string filePath = "nonexistent.json";
            var loggerMock = new Mock<ILogger>();

            // Act
            var result = JsonHelper.LoadJsonFile<List<InventoryLogEntry>>(filePath, loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains(filePath))), Times.Once);
        }

        [Fact]
        public void LoadJsonFile_InvalidJson_ReturnsDefault()
        {
            // Arrange
            string filePath = "test_invalid.json";
            string invalidJson = "{ invalid json ";
            File.WriteAllText(filePath, invalidJson);
            var loggerMock = new Mock<ILogger>();

            // Act
            var result = JsonHelper.LoadJsonFile<List<InventoryLogEntry>>(filePath, loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains(filePath))), Times.Once);

            // Cleanup
            File.Delete(filePath);
        }
    }
}
