using Newtonsoft.Json;

namespace SecureLens
{
    public class InventoryLogEntry
    {
        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("inventoryAvailable")]
        public bool? InventoryAvailable { get; set; }

        [JsonProperty("inventoryDate")]
        public DateTime? InventoryDate { get; set; }

        [JsonProperty("abrClientVersion")]
        public string? AbrClientVersion { get; set; }

        [JsonProperty("abrClientInstallDate")]
        public DateTime? AbrClientInstallDate { get; set; }

        [JsonProperty("notes")]
        public string? Notes { get; set; }

        [JsonProperty("user")]
        public User? User { get; set; }

        [JsonProperty("owner")]
        public Owner? Owner { get; set; }

        [JsonProperty("computer")]
        public Computer? Computer { get; set; }

        [JsonProperty("operatingSystem")]
        public OperatingSystem? OperatingSystem { get; set; }

        [JsonProperty("hardware")]
        public Hardware? Hardware { get; set; }

        [JsonProperty("network")]
        public Network? Network { get; set; }

        [JsonProperty("location")]
        public Location? Location { get; set; }

        [JsonProperty("software")]
        public List<Software>? Software { get; set; }
    }

    public class User
    {
        [JsonProperty("account")]
        public string? Account { get; set; }

        [JsonProperty("fullName")]
        public string? FullName { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("phone")]
        public string? Phone { get; set; }

        [JsonProperty("domain")]
        public string? Domain { get; set; }

        [JsonProperty("orgUnit")]
        public string? OrgUnit { get; set; }

        [JsonProperty("orgUnitPath")]
        public string? OrgUnitPath { get; set; }

        [JsonProperty("isAdmin")]
        public bool? IsAdmin { get; set; }

        [JsonProperty("isDomainJoined")]
        public bool? IsDomainJoined { get; set; }

        [JsonProperty("isAzureJoined")]
        public bool? IsAzureJoined { get; set; }

        [JsonProperty("groups")]
        public List<string>? Groups { get; set; }
    }

    public class Owner
    {
        [JsonProperty("account")]
        public string? Account { get; set; }

        [JsonProperty("fullName")]
        public string? FullName { get; set; }
    }

    public class Computer
    {
        [JsonProperty("domain")]
        public string? Domain { get; set; }

        [JsonProperty("isDomainJoined")]
        public bool? IsDomainJoined { get; set; }

        [JsonProperty("isAzureJoined")]
        public bool? IsAzureJoined { get; set; }

        [JsonProperty("orgUnit")]
        public string? OrgUnit { get; set; }

        [JsonProperty("orgUnitPath")]
        public string? OrgUnitPath { get; set; }

        [JsonProperty("groups")]
        public List<string>? Groups { get; set; }

        [JsonProperty("localAdmins")]
        public List<string>? LocalAdmins { get; set; }

        [JsonProperty("users")]
        public List<string>? Users { get; set; }
    }

    public class OperatingSystem
    {
        [JsonProperty("platform")]
        public string? Platform { get; set; }

        [JsonProperty("platformCode")]
        public int? PlatformCode { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("version")]
        public string? Version { get; set; }

        [JsonProperty("release")]
        public int? Release { get; set; }

        [JsonProperty("build")]
        public int? Build { get; set; }

        [JsonProperty("buildUpdate")]
        public int? BuildUpdate { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("typeCode")]
        public int? TypeCode { get; set; }

        [JsonProperty("bits")]
        public int? Bits { get; set; }

        [JsonProperty("installDate")]
        public DateTime? InstallDate { get; set; }
    }

    public class Hardware
    {
        [JsonProperty("make")]
        public string? Make { get; set; }

        [JsonProperty("model")]
        public string? Model { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("typeCode")]
        public int? TypeCode { get; set; }

        [JsonProperty("serviceTag")]
        public string? ServiceTag { get; set; }

        [JsonProperty("cpu")]
        public string? Cpu { get; set; }

        [JsonProperty("cpuSpeed")]
        public int? CpuSpeed { get; set; }

        [JsonProperty("cpuCores")]
        public int? CpuCores { get; set; }

        [JsonProperty("diskSize")]
        public int? DiskSize { get; set; }

        [JsonProperty("diskFree")]
        public int? DiskFree { get; set; }

        [JsonProperty("diskStatus")]
        public string? DiskStatus { get; set; }

        [JsonProperty("memory")]
        public int? Memory { get; set; }

        [JsonProperty("noMonitors")]
        public int? NoMonitors { get; set; }

        [JsonProperty("monitorResolution")]
        public string? MonitorResolution { get; set; }

        [JsonProperty("bitlockerEnabled")]
        public bool? BitlockerEnabled { get; set; }

        [JsonProperty("isCompliant")]
        public bool? IsCompliant { get; set; }

        [JsonProperty("tpmEnabled")]
        public bool? TpmEnabled { get; set; }

        [JsonProperty("tpmVersion")]
        public string? TpmVersion { get; set; }
    }

    public class Network
    {
        [JsonProperty("publicIP")]
        public string? PublicIP { get; set; }

        [JsonProperty("privateIP")]
        public string? PrivateIP { get; set; }

        [JsonProperty("macAddress")]
        public string? MacAddress { get; set; }

        [JsonProperty("nicSpeed")]
        public string? NicSpeed { get; set; }

        [JsonProperty("hostName")]
        public string? HostName { get; set; }
    }

    public class Location
    {
        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("region")]
        public string? Region { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("latitude")]
        public string? Latitude { get; set; }

        [JsonProperty("longitude")]
        public string? Longitude { get; set; }

        [JsonProperty("googleMapsLink")]
        public string? GoogleMapsLink { get; set; }

        [JsonProperty("hourOffset")]
        public int? HourOffset { get; set; }
    }

    public class Software
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("version")]
        public string? Version { get; set; }

        [JsonProperty("vendor")]
        public string? Vendor { get; set; }

        [JsonProperty("installDate")]
        public DateTime? InstallDate { get; set; }

        [JsonProperty("size")]
        public int? Size { get; set; }

        [JsonProperty("bits")]
        public int? Bits { get; set; }
    }
}
