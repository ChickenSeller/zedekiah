using System.Text.Json.Serialization;

namespace Zedkiah.dto.zerotier.center;

public class NetworkMember
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("clock")]
    public long Clock { get; set; }

    [JsonPropertyName("networkId")]
    public string NetworkId { get; set; }

    [JsonPropertyName("nodeId")]
    public string NodeId { get; set; }

    [JsonPropertyName("controllerId")]
    public string ControllerId { get; set; }

    [JsonPropertyName("hidden")]
    public bool Hidden { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("config")]
    public MemberConfig Config { get; set; }

    [JsonPropertyName("lastOnline")]
    public long LastOnline { get; set; }

    [JsonPropertyName("lastSeen")]
    public long LastSeen { get; set; }

    [JsonPropertyName("physicalAddress")]
    public string PhysicalAddress { get; set; }

    [JsonPropertyName("physicalLocation")]
    public object PhysicalLocation { get; set; } // 可以根据实际结构调整类型

    [JsonPropertyName("clientVersion")]
    public string ClientVersion { get; set; }

    [JsonPropertyName("protocolVersion")]
    public int ProtocolVersion { get; set; }

    [JsonPropertyName("supportsRulesEngine")]
    public bool SupportsRulesEngine { get; set; }

    [JsonPropertyName("arch")]
    public string Arch { get; set; }

    [JsonPropertyName("os")]
    public string Os { get; set; }
}

public class MemberConfig
{
    [JsonPropertyName("activeBridge")]
    public bool ActiveBridge { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("authorized")]
    public bool Authorized { get; set; }

    [JsonPropertyName("capabilities")]
    public List<string> Capabilities { get; set; }

    [JsonPropertyName("creationTime")]
    public long CreationTime { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("identity")]
    public string Identity { get; set; }

    [JsonPropertyName("ipAssignments")]
    public List<string> IpAssignments { get; set; }

    [JsonPropertyName("lastAuthorizedTime")]
    public long LastAuthorizedTime { get; set; }

    [JsonPropertyName("lastDeauthorizedTime")]
    public long LastDeauthorizedTime { get; set; }

    [JsonPropertyName("noAutoAssignIps")]
    public bool NoAutoAssignIps { get; set; }

    [JsonPropertyName("nwid")]
    public string Nwid { get; set; }

    [JsonPropertyName("objtype")]
    public string ObjType { get; set; }

    [JsonPropertyName("remoteTraceLevel")]
    public int RemoteTraceLevel { get; set; }

    [JsonPropertyName("remoteTraceTarget")]
    public string RemoteTraceTarget { get; set; }

    [JsonPropertyName("revision")]
    public int Revision { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    [JsonPropertyName("vMajor")]
    public int VMajor { get; set; }

    [JsonPropertyName("vMinor")]
    public int VMinor { get; set; }

    [JsonPropertyName("vRev")]
    public int VRev { get; set; }

    [JsonPropertyName("vProto")]
    public int VProto { get; set; }

    [JsonPropertyName("ssoExempt")]
    public bool SsoExempt { get; set; }
}