using Xunit;
using SecureLens.Core.Models;
using System.Collections.Generic;

namespace SecureLens.Tests.Core.Models
{
    public class CompletedUserTests
    {
        [Fact]
        public void AddAuditLogEntry_AddsEntryToList()
        {
            // Arrange
            var user = new CompletedUser { AccountName = "user1" };
            var auditEntry = new AuditLogEntry { /* Initialize properties */ };

            // Act
            user.AddAuditLogEntry(auditEntry);

            // Assert
            Assert.Single(user.AuditLogEntries);
            Assert.Contains(auditEntry, user.AuditLogEntries);
        }

        [Fact]
        public void AddInventoryLogEntry_AddsEntryToList()
        {
            // Arrange
            var user = new CompletedUser { AccountName = "user1" };
            var inventoryEntry = new InventoryLogEntry { /* Initialize properties */ };

            // Act
            user.AddInventoryLogEntry(inventoryEntry);

            // Assert
            Assert.Single(user.InventoryLogEntries);
            Assert.Contains(inventoryEntry, user.InventoryLogEntries);
        }

        [Fact]
        public void ActiveDirectoryUser_DefaultsToNull()
        {
            // Arrange
            var user = new CompletedUser { AccountName = "user1" };

            // Act & Assert
            Assert.Null(user.ActiveDirectoryUser);
        }
    }
}