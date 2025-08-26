using System.Text.Json.Serialization;

namespace Zedkiah.dto.zerotier.center;

public class NodeName
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}