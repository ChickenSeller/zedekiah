using System.Text.Json.Serialization;

namespace Zedkiah.dto.zerotier;

public class ZeroTierPeerInfo
{
    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("isBonded")]
    public bool IsBonded { get; set; }

    [JsonPropertyName("latency")]
    public int Latency { get; set; }

    [JsonPropertyName("paths")]
    public List<PeerPath> Paths { get; set; }

    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("tunneled")]
    public bool Tunneled { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("versionMajor")]
    public int VersionMajor { get; set; }

    [JsonPropertyName("versionMinor")]
    public int VersionMinor { get; set; }

    [JsonPropertyName("versionRev")]
    public int VersionRev { get; set; }
}

public class PeerPath
{
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("expired")]
    public bool Expired { get; set; }

    [JsonPropertyName("lastReceive")]
    public long LastReceive { get; set; }

    [JsonPropertyName("lastSend")]
    public long LastSend { get; set; }

    [JsonPropertyName("localSocket")]
    public long LocalSocket { get; set; }

    [JsonPropertyName("preferred")]
    public bool Preferred { get; set; }

    [JsonPropertyName("trustedPathId")]
    public int TrustedPathId { get; set; }
}