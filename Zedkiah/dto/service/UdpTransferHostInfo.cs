using System.Text.Json.Serialization;

namespace Zedkiah.dto.service;

public class UdpTransferHostInfo:UdpMessage
{
    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }
    [JsonPropertyName("host_name")]
    public string HostName { get; set; }
    [JsonPropertyName("proxy_enabled")]
    public bool ProxyEnabled { get; set; }

    [JsonPropertyName("proxy_port")] 
    public int ProxyPort = -1;
    [JsonPropertyName("ip_address")]
    public String IpAddress { get; set; } = "";
}