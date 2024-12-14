using Xunit;
using SecureLens;
using System.Collections.Generic;

namespace SecureLens.Tests
{
    public class DataSanitizerTests
    {
        [Fact]
        public void SanitizeAuditLogs_NullsPersonalFields()
        {
            // Arrange
            var auditLogs = new List<AuditLogEntry>
            {
                new AuditLogEntry
                {
                    User = new AuditUser
                    {
                        FullName = "John Doe",
                        Email = "john.doe@example.com",
                        Phone = "123-456-7890"
                    },
                    ApprovedBy = "Admin User",
                    DeniedBy = "Admin Denier"
                }
            };

            // Act
            DataSanitizer.SanitizeAuditLogs(auditLogs);

            // Assert
            foreach (var log in auditLogs)
            {
                Assert.Null(log.User.FullName);
                Assert.Null(log.User.Email);
                Assert.Null(log.User.Phone);
                Assert.Null(log.ApprovedBy);
                Assert.Null(log.DeniedBy);
            }
        }

        [Fact]
        public void SanitizeInventoryLogs_NullsPersonalFields()
        {
            // Arrange
            var inventoryLogs = new List<InventoryLogEntry>
            {
                new InventoryLogEntry
                {
                    User = new User
                    {
                        FullName = "Jane Smith",
                        Email = "jane.smith@example.com",
                        Phone = "987-654-3210"
                    },
                    Owner = new Owner
                    {
                        FullName = "Owner Name"
                    },
                    Location = new Location
                    {
                        City = "New York",
                        Region = "NY",
                        Country = "USA",
                        Latitude = "40.7128 N",
                        Longitude = "74.0060 W",
                        GoogleMapsLink = "https://maps.google.com/?q=40.7128,-74.0060",
                        HourOffset = -5
                    },
                    Network = new Network
                    {
                        PublicIP = "192.0.2.1",
                        PrivateIP = "10.0.0.1",
                        MacAddress = "00:1A:2B:3C:4D:5E",
                        NicSpeed = "1 Gbps",
                        HostName = "HOST123"
                    }
                }
            };

            // Act
            DataSanitizer.SanitizeInventoryLogs(inventoryLogs);

            // Assert
            foreach (var log in inventoryLogs)
            {
                Assert.Null(log.User.FullName);
                Assert.Null(log.User.Email);
                Assert.Null(log.User.Phone);
                Assert.Null(log.Owner.FullName);
                Assert.Null(log.Location.City);
                Assert.Null(log.Location.Region);
                Assert.Null(log.Location.Country);
                Assert.Null(log.Location.Latitude);
                Assert.Null(log.Location.Longitude);
                Assert.Null(log.Location.GoogleMapsLink);
                Assert.Null(log.Location.HourOffset);
                Assert.Null(log.Network.PublicIP);
                Assert.Null(log.Network.PrivateIP);
                Assert.Null(log.Network.MacAddress);
                Assert.Null(log.Network.NicSpeed);
                Assert.Null(log.Network.HostName);
            }
        }
    }
}
