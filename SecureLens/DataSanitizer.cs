using System.Collections.Generic;

namespace SecureLens
{
    public static class DataSanitizer
    {
        /// <summary>
        /// Sanitizes a list of AuditLogEntry by nullifying personal information.
        /// </summary>
        /// <param name="auditLogs">List of AuditLogEntry to sanitize.</param>
        public static void SanitizeAuditLogs(List<AuditLogEntry> auditLogs)
        {
            foreach (var log in auditLogs)
            {
                // Sanitize User Information
                if (log.User != null)
                {
                    log.User.FullName = null;
                    log.User.Email = null;
                    log.User.Phone = null;
                }
            }
        }

        /// <summary>
        /// Sanitizes a list of InventoryLogEntry by nullifying personal information.
        /// </summary>
        /// <param name="inventoryLogs">List of InventoryLogEntry to sanitize.</param>
        public static void SanitizeInventoryLogs(List<InventoryLogEntry> inventoryLogs)
        {
            foreach (var log in inventoryLogs)
            {
                // Sanitize User Information
                if (log.User != null)
                {
                    log.User.FullName = null;
                    log.User.Email = null;
                    log.User.Phone = null;
                }

                // Sanitize Owner Information
                if (log.Owner != null)
                {
                    log.Owner.FullName = null;
                }

                // Sanitize Location Information
                if (log.Location != null)
                {
                    log.Location.City = null;
                    log.Location.Region = null;
                    log.Location.Country = null;
                    log.Location.Latitude = null;
                    log.Location.Longitude = null;
                    log.Location.GoogleMapsLink = null;
                    log.Location.HourOffset = null;
                }

                // Sanitize Network Information
                if (log.Network != null)
                {
                    log.Network.PublicIP = null;
                    log.Network.PrivateIP = null;
                    log.Network.MacAddress = null;
                    log.Network.NicSpeed = null;
                    log.Network.HostName = null;
                }
                
            }
        }
    }
}
