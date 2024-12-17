using Xunit;
using Moq;
using SecureLens.Infrastructure.Logging;
using SecureLens.Infrastructure.Strategies;
using SecureLens.Infrastructure.Interfaces;
using System.Collections.Generic;

namespace SecureLens.Tests.Infrastructure.Strategies
{
    public class ActiveDirectoryCacheStrategyTests
    {
        [Fact]
        public void QueryAdGroup_GroupExists_ReturnsMembers()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var strategy = new ActiveDirectoryCacheStrategy(loggerMock.Object);

            var groupCache = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Group1", new List<string> { "user1", "user2" } },
                { "Group2", new List<string> { "user3" } }
            };
            strategy.InitializeGroupCache(groupCache);

            // Act
            var members = strategy.QueryAdGroup("Group1");

            // Assert
            Assert.NotNull(members);
            Assert.Equal(2, members.Count);
            Assert.Contains("user1", members);
            Assert.Contains("user2", members);
        }

        [Fact]
        public void QueryAdGroup_GroupDoesNotExist_ReturnsEmptyListAndLogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var strategy = new ActiveDirectoryCacheStrategy(loggerMock.Object);

            var groupCache = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Group1", new List<string> { "user1", "user2" } }
            };
            strategy.InitializeGroupCache(groupCache);

            // Act
            var members = strategy.QueryAdGroup("NonExistentGroup");

            // Assert
            Assert.Empty(members);
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("not found in cache"))), Times.Once);
        }

        [Fact]
        public void QueryAdGroupMembers_MultipleGroups_ReturnsUniqueMembers()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var strategy = new ActiveDirectoryCacheStrategy(loggerMock.Object);

            var groupCache = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Group1", new List<string> { "user1", "user2" } },
                { "Group2", new List<string> { "user2", "user3" } },
                { "Group3", new List<string> { "user4" } }
            };
            strategy.InitializeGroupCache(groupCache);

            var groupNames = new List<string> { "Group1", "Group2", "Group3" };

            // Act
            var allMembers = strategy.QueryAdGroupMembers(groupNames);

            // Assert
            Assert.NotNull(allMembers);
            Assert.Equal(4, allMembers.Count);
            Assert.Contains("user1", allMembers);
            Assert.Contains("user2", allMembers);
            Assert.Contains("user3", allMembers);
            Assert.Contains("user4", allMembers);
        }

        [Fact]
        public void QueryAdGroupMembers_SomeGroupsDoNotExist_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var strategy = new ActiveDirectoryCacheStrategy(loggerMock.Object);

            var groupCache = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Group1", new List<string> { "user1" } },
                { "Group2", new List<string> { "user2" } }
            };
            strategy.InitializeGroupCache(groupCache);

            var groupNames = new List<string> { "Group1", "NonExistentGroup" };

            // Act
            var allMembers = strategy.QueryAdGroupMembers(groupNames);

            // Assert
            Assert.Single(allMembers);
            Assert.Contains("user1", allMembers);
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("groups not found"))), Times.Once);
        }
    }
}
