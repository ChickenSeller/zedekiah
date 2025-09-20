using System.IO;
using System.Net;
using System.Text.Json.Serialization;
using System.Windows;

namespace Zedkiah.dto;

public class ZeroTierCenterConfig
{
    [JsonPropertyName("api_key")]
    public string ApiKey { get; set; }
    [JsonPropertyName("network_id")]
    public string NetworkID { get; set; }
    [JsonIgnore]
    public static ZeroTierCenterConfig Config { get; private set; }
    
    private static string FilePath = "ztcenter_config.json";

    public static void Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return;
            }
            var json = File.ReadAllText(FilePath);
            Config = System.Text.Json.JsonSerializer.Deserialize<ZeroTierCenterConfig>(json);
            EnvironmentInfo.NetworkId = Config.NetworkID;
            
        }
        catch
        {
            Environment.Exit(1);
        }
    }
}