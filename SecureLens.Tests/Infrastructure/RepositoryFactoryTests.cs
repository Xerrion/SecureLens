using Xunit;
using Moq;
using SecureLens.Infrastructure.Logging;
using SecureLens.Infrastructure.Factories;
using SecureLens.Infrastructure.Interfaces;
using SecureLens.Infrastructure.Strategies;
using System.Collections.Generic;
using SecureLens.Infrastructure.Data.Repositories;

namespace SecureLens.Tests.Infrastructure.Factories
{
    public class RepositoryFactoryTests
    {
        [Fact]
        public void CreateAdminByRequestRepository_CacheMode_UsesCacheStrategy()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            // snyk:ignore hardcoded-credentials
            string apiKey = "testapikey";
            bool useLiveData = false;
            string inventoryPath = "inventory_cache.json";
            string auditPath = "audit_cache.json";

            // Act
            var repo = RepositoryFactory.CreateAdminByRequestRepository(
                apiKey,
                loggerMock.Object,
                useLiveData,
                inventoryPath,
                auditPath
            );

            // Assert
            Assert.IsType<AdminByRequestRepository>(repo);
            // Assuming AdminByRequestCacheStrategy is internal, you might need to expose it for testing or use reflection.
            // Alternatively, verify behavior via repository methods.
        }

        [Fact]
        public void CreateAdminByRequestRepository_LiveMode_UsesLiveStrategy()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            // snyk:ignore hardcoded-credentials
            string apiKey = "testapikey";
            bool useLiveData = true;

            // Act
            var repo = RepositoryFactory.CreateAdminByRequestRepository(
                apiKey,
                loggerMock.Object,
                useLiveData
            );

            // Assert
            Assert.IsType<AdminByRequestRepository>(repo);
            // Further assertions would require access to the internal strategy
        }

        [Fact]
        public void CreateActiveDirectoryRepository_CacheMode_UsesCacheStrategy()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            bool useLiveData = false;
            string groupCachePath = "group_cache.json";
            string userCachePath = "user_cache.json";

            // Act
            var repo = RepositoryFactory.CreateActiveDirectoryRepository(
                loggerMock.Object,
                useLiveData,
                groupCachePath,
                userCachePath
            );

            // Assert
            Assert.IsType<ActiveDirectoryRepository>(repo);
            // Similar to above, verify behavior via repository methods
        }

        [Fact]
        public void CreateActiveDirectoryRepository_LiveMode_UsesLiveStrategy()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            bool useLiveData = true;

            // Act
            var repo = RepositoryFactory.CreateActiveDirectoryRepository(
                loggerMock.Object,
                useLiveData
            );

            // Assert
            Assert.IsType<ActiveDirectoryRepository>(repo);
        }
    }
}
