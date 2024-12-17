using Xunit;
using Moq;
using SecureLens.Infrastructure.Logging;
using SecureLens.Infrastructure.Strategies;
using SecureLens.Infrastructure.Interfaces;
using System.Collections.Generic;
using SecureLens.Core.Models;
using System.IO;

namespace SecureLens.Tests.Infrastructure.Strategies
{
    public class AdminByRequestCacheStrategyTests
    {
        [Fact]
        public void LoadCachedInventoryData_ValidFile_ReturnsData()
        {
            // Arrange
            string filePath = "inventory_cache.json";
            var expectedData = new List<InventoryLogEntry>
            {
                new InventoryLogEntry { /* Initialize properties */ },
                new InventoryLogEntry { /* Initialize properties */ }
            };
            string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(expectedData);
            File.WriteAllText(filePath, jsonContent);
            var loggerMock = new Mock<ILogger>();
            var strategy = new AdminByRequestCacheStrategy(loggerMock.Object);

            // Act
            var result = strategy.LoadCachedInventoryData(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public void LoadCachedAuditLogs_ValidFile_ReturnsData()
        {
            // Arrange
            string filePath = "audit_cache.json";
            var expectedData = new List<AuditLogEntry>
            {
                new AuditLogEntry { /* Initialize properties */ },
                new AuditLogEntry { /* Initialize properties */ }
            };
            string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(expectedData);
            File.WriteAllText(filePath, jsonContent);
            var loggerMock = new Mock<ILogger>();
            var strategy = new AdminByRequestCacheStrategy(loggerMock.Object);

            // Act
            var result = strategy.LoadCachedAuditLogs(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public void FetchInventoryDataAsync_NotSupported_ReturnsEmptyList()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var strategy = new AdminByRequestCacheStrategy(loggerMock.Object);
            string inventoryUrl = "https://api.example.com/inventory";
            var headers = new Dictionary<string, string>();

            // Act
            var task = strategy.FetchInventoryDataAsync(inventoryUrl, headers);
            task.Wait();
            var result = task.Result;

            // Assert
            Assert.Empty(result);
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("not supported"))), Times.Once);
        }

        [Fact]
        public void FetchAuditLogsAsync_NotSupported_ReturnsEmptyList()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var strategy = new AdminByRequestCacheStrategy(loggerMock.Object);
            string auditUrl = "https://api.example.com/audit";
            var headers = new Dictionary<string, string>();
            var parameters = new Dictionary<string, string>();

            // Act
            var task = strategy.FetchAuditLogsAsync(auditUrl, headers, parameters);
            task.Wait();
            var result = task.Result;

            // Assert
            Assert.Empty(result);
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("not supported"))), Times.Once);
        }
    }
}
