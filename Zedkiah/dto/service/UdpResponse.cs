using System.Text.Json.Serialization;

namespace Zedkiah.dto.service;

public class UdpResponse
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("content")]
    public UdpMessage Content { get; set; }
}