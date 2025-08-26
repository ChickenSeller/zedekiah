using System.Text.Json.Serialization;

namespace Zedkiah.dto.zerotier;

public class Settings
{
    [JsonPropertyName("allowGlobal")]
    public bool AllowGlobal { get; set; }

    [JsonPropertyName("allowTcpFallbackRelay")]
    public bool AllowTcpFallbackRelay { get; set; }

    [JsonPropertyName("forceTcpRelay")]
    public bool ForceTcpRelay { get; set; }

    [JsonPropertyName("listeningOn")]
    public List<string> ListeningOn { get; set; }

    [JsonPropertyName("portMappingEnabled")]
    public bool PortMappingEnabled { get; set; }

    [JsonPropertyName("primaryPort")]
    public int PrimaryPort { get; set; }

    [JsonPropertyName("secondaryPort")]
    public int SecondaryPort { get; set; }

    [JsonPropertyName("softwareUpdate")]
    public string SoftwareUpdate { get; set; }

    [JsonPropertyName("softwareUpdateChannel")]
    public string SoftwareUpdateChannel { get; set; }

    [JsonPropertyName("surfaceAddresses")]
    public List<string> SurfaceAddresses { get; set; }

    [JsonPropertyName("tertiaryPort")]
    public int TertiaryPort { get; set; }
}

public class Config
{
    [JsonPropertyName("settings")]
    public Settings Settings { get; set; }
}

public class SelfInfo
{
    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("clock")]
    public long Clock { get; set; }

    [JsonPropertyName("config")]
    public Config Config { get; set; }

    [JsonPropertyName("online")]
    public bool Online { get; set; }

    [JsonPropertyName("planetWorldId")]
    public long PlanetWorldId { get; set; }

    [JsonPropertyName("planetWorldTimestamp")]
    public long PlanetWorldTimestamp { get; set; }

    [JsonPropertyName("publicIdentity")]
    public string PublicIdentity { get; set; }

    [JsonPropertyName("tcpFallbackActive")]
    public bool TcpFallbackActive { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("versionBuild")]
    public int VersionBuild { get; set; }

    [JsonPropertyName("versionMajor")]
    public int VersionMajor { get; set; }

    [JsonPropertyName("versionMinor")]
    public int VersionMinor { get; set; }

    [JsonPropertyName("versionRev")]
    public int VersionRev { get; set; }
}