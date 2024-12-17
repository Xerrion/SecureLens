using Newtonsoft.Json;

namespace SecureLens.Core.Models;

public class AuditLogEntry
{
    [JsonProperty("id")] public int? Id { get; set; }

    [JsonProperty("traceNo")] public string? TraceNo { get; set; }

    [JsonProperty("settingsName")] public string? SettingsName { get; set; }

    [JsonProperty("type")] public string? Type { get; set; }

    [JsonProperty("typeCode")] public int? TypeCode { get; set; }

    [JsonProperty("status")] public string? Status { get; set; }

    [JsonProperty("statusCode")] public int? StatusCode { get; set; }

    [JsonProperty("reason")] public string? Reason { get; set; }

    [JsonProperty("approvedBy")] public string? ApprovedBy { get; set; }

    [JsonProperty("deniedReason")] public string? DeniedReason { get; set; }

    [JsonProperty("deniedBy")] public string? DeniedBy { get; set; }

    [JsonProperty("ssoValidated")] public bool? SsoValidated { get; set; }

    [JsonProperty("requestTime")] public DateTime? RequestTime { get; set; }

    [JsonProperty("requestTimeUTC")] public DateTime? RequestTimeUTC { get; set; }

    [JsonProperty("responseTime")] public TimeSpan? ResponseTime { get; set; }

    [JsonProperty("startTime")] public DateTime? StartTime { get; set; }

    [JsonProperty("startTimeUTC")] public DateTime? StartTimeUTC { get; set; }

    [JsonProperty("endTime")] public DateTime? EndTime { get; set; }

    [JsonProperty("endTimeUTC")] public DateTime? EndTimeUTC { get; set; }

    [JsonProperty("auditlogLink")] public string? AuditlogLink { get; set; }

    [JsonProperty("user")] public AuditUser? User { get; set; }

    [JsonProperty("computer")] public AuditComputer? Computer { get; set; }

    [JsonProperty("application")] public AuditApplication? Application { get; set; }

    [JsonProperty("installs")] public List<AuditInstall>? Installs { get; set; }

    [JsonProperty("uninstalls")] public List<AuditUninstall>? Uninstalls { get; set; }

    [JsonProperty("elevatedApplications")] public List<AuditElevatedApplication>? ElevatedApplications { get; set; }

    [JsonProperty("scanResults")] public List<AuditScanResult>? ScanResults { get; set; }
}

public class AuditUser
{
    [JsonProperty("account")] public string? Account { get; set; }

    [JsonProperty("fullName")] public string? FullName { get; set; }

    [JsonProperty("email")] public string? Email { get; set; }

    [JsonProperty("phone")] public string? Phone { get; set; }

    [JsonProperty("isAdmin")] public bool? IsAdmin { get; set; }
}

public class AuditComputer
{
    [JsonProperty("name")] public string? Name { get; set; }

    [JsonProperty("platform")] public string? Platform { get; set; }

    [JsonProperty("platformCode")] public int? PlatformCode { get; set; }

    [JsonProperty("make")] public string? Make { get; set; }

    [JsonProperty("model")] public string? Model { get; set; }
}

public class AuditApplication
{
    [JsonProperty("file")] public string? File { get; set; }

    [JsonProperty("path")] public string? Path { get; set; }

    [JsonProperty("name")] public string? Name { get; set; }

    [JsonProperty("vendor")] public string? Vendor { get; set; }

    [JsonProperty("version")] public string? Version { get; set; }

    [JsonProperty("sha256")] public string? Sha256 { get; set; }

    [JsonProperty("scanResult")] public string? ScanResult { get; set; }

    [JsonProperty("scanResultCode")] public int? ScanResultCode { get; set; }

    [JsonProperty("threat")] public string? Threat { get; set; }

    [JsonProperty("virustotalLink")] public string? VirustotalLink { get; set; }

    [JsonProperty("preapproved")] public bool? Preapproved { get; set; }
}

public class AuditInstall
{
    [JsonProperty("application")] public string? Application { get; set; }

    [JsonProperty("version")] public string? Version { get; set; }

    [JsonProperty("vendor")] public string? Vendor { get; set; }
}

public class AuditUninstall
{
    [JsonProperty("application")] public string? Application { get; set; }

    [JsonProperty("version")] public string? Version { get; set; }

    [JsonProperty("vendor")] public string? Vendor { get; set; }
}

public class AuditElevatedApplication
{
    [JsonProperty("name")] public string? Name { get; set; }

    [JsonProperty("path")] public string? Path { get; set; }

    [JsonProperty("file")] public string? File { get; set; }

    [JsonProperty("version")] public string? Version { get; set; }

    [JsonProperty("vendor")] public string? Vendor { get; set; }

    [JsonProperty("sha256")] public string? Sha256 { get; set; }

    [JsonProperty("scanResult")] public string? ScanResult { get; set; }

    [JsonProperty("scanResultCode")] public int? ScanResultCode { get; set; }

    [JsonProperty("threat")] public string? Threat { get; set; }

    [JsonProperty("virustotalLink")] public string? VirustotalLink { get; set; }
}

public class AuditScanResult
{
    [JsonProperty("scanResult")] public string? ScanResult { get; set; }

    [JsonProperty("scanResultCode")] public int? ScanResultCode { get; set; }

    [JsonProperty("engine")] public string? Engine { get; set; }

    [JsonProperty("threat")] public string? Threat { get; set; }
}