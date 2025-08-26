using System.Text.Json.Serialization;

namespace Zedkiah.dto.zerotier;

public class NetworkInfo
{
    [JsonPropertyName("allowDNS")]
    public bool AllowDNS { get; set; }

    [JsonPropertyName("allowDefault")]
    public bool AllowDefault { get; set; }

    [JsonPropertyName("allowGlobal")]
    public bool AllowGlobal { get; set; }

    [JsonPropertyName("allowManaged")]
    public bool AllowManaged { get; set; }

    [JsonPropertyName("assignedAddresses")]
    public List<string> AssignedAddresses { get; set; }

    [JsonPropertyName("bridge")]
    public bool Bridge { get; set; }

    [JsonPropertyName("broadcastEnabled")]
    public bool BroadcastEnabled { get; set; }

    [JsonPropertyName("dhcp")]
    public bool Dhcp { get; set; }

    [JsonPropertyName("dns")]
    public DnsInfo Dns { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("mac")]
    public string Mac { get; set; }

    [JsonPropertyName("mtu")]
    public int Mtu { get; set; }

    [JsonPropertyName("multicastSubscriptions")]
    public List<MulticastSubscription> MulticastSubscriptions { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("netconfRevision")]
    public long NetconfRevision { get; set; }

    [JsonPropertyName("nwid")]
    public string Nwid { get; set; }

    [JsonPropertyName("portDeviceName")]
    public string PortDeviceName { get; set; }

    [JsonPropertyName("portError")]
    public int PortError { get; set; }

    [JsonPropertyName("routes")]
    public List<RouteInfo> Routes { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public class DnsInfo
{
    [JsonPropertyName("domain")]
    public string Domain { get; set; }

    [JsonPropertyName("servers")]
    public List<string> Servers { get; set; }
}

public class MulticastSubscription
{
    [JsonPropertyName("adi")]
    public long Adi { get; set; }

    [JsonPropertyName("mac")]
    public string Mac { get; set; }
}

public class RouteInfo
{
    [JsonPropertyName("flags")]
    public int Flags { get; set; }

    [JsonPropertyName("metric")]
    public int Metric { get; set; }

    [JsonPropertyName("target")]
    public string Target { get; set; }

    [JsonPropertyName("via")]
    public string Via { get; set; }
}