using System.Text.Json.Serialization;

namespace Zedkiah.dto.service;

public class UdpQuery
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("host_info")]
    public UdpTransferHostInfo HostInfo { get; set; }
}