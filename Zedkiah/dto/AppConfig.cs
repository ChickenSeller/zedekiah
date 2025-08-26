using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Zedkiah.zerotier;

namespace Zedkiah.dto;

public class AppConfig
{
    [JsonPropertyName("auto_start_gui")]
    public bool AutoStartGui { get; set; } = false;
    [JsonPropertyName("auto_start_proxy")]
    public bool AutoStartProxy { get; set; } = false;
    [JsonPropertyName("auto_start_zerotier")]
    public bool AutoStartZeroTier { get; set; } = false;
    [JsonPropertyName("network_id")]
    public string NetworkId { get; set; } = "8056c2e21c96887f";
    [JsonPropertyName("proxy_port")]
    public int ProxyPort { get; set; } = 10086;
    [JsonPropertyName("managed_devices")]
    public List<string> ManagedDevices { get; set; } = [];
    [JsonIgnore]
    public int ZeroTierInitStatus { get; set;}
    [JsonIgnore]
    private bool _isNew;
    [JsonIgnore]
    public string Hostname = Environment.MachineName;
    public static bool Save(AppConfig config)
    {
        try
        {
            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("config.json", json);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving config: {ex.Message}");
            return false;
        }
    }
    public static AppConfig Load()
    {
        try
        {
            
            var config = new AppConfig();
            if (File.Exists("config.json"))
            {
                var json = File.ReadAllText("config.json");
                config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            else
            {
                config._isNew = true;
            }
            config.ZeroTierInitStatus = ZeroTierManager.GetInitStatus(config.NetworkId);
            if (config._isNew)
            {
                if (ZeroTierManager.GetInitStatus(config.NetworkId) == 2)
                {
                    config.AutoStartZeroTier = true;
                }
            }
            config._isNew = false;
            if(config.ProxyPort<1 || config.ProxyPort > 65535)
            {
                config.ProxyPort = 10086;
            }
            Save(config);
            return config;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config: {ex.Message}");
            return new AppConfig();
        }
    }
}