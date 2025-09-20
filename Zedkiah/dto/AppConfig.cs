using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Zedkiah.zerotier;

namespace Zedkiah.dto;

public class AppConfig
{
    public static int ZeroTierInitStatus { get; set;}
    private static bool _isNew;
    public static InternalAppConfig Config { get; set;}
    
    public static bool Save()
    {
        if (Config == null)
        {
            Config = new InternalAppConfig();
        }
        try
        {
            string json =
                JsonSerializer.Serialize(Config, new JsonSerializerOptions {WriteIndented = true});
            File.WriteAllText("config.json", json);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving config: {ex.Message}");
            return false;
        }
    }
    public static void Load()
    {
        try
        {
            if (!File.Exists("config.json"))
            {
                _isNew = true;
                Save();
                Load();
                return;
            }
            Config = JsonSerializer.Deserialize<InternalAppConfig>(File.ReadAllText("config.json"));
            // config.NetworkId = ZeroTierCenterConfig.Config.NetworkID;
            ZeroTierInitStatus = ZeroTierManager.GetInitStatus(Config.NetworkId);
            if (_isNew)
            {
                if (ZeroTierManager.GetInitStatus(Config.NetworkId) == 2)
                {
                    Config.AutoStartZeroTier = true;
                }
            }
            _isNew = false;
            if(Config.ProxyPort<1 || Config.ProxyPort > 65535)
            {
                Config.ProxyPort = 10086;
            }
            Save();
            _isNew = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config: {ex.Message}");
        }
    }
}

public class InternalAppConfig
{
    [JsonPropertyName("auto_start_gui")]
    public bool AutoStartGui { get; set; } = false;
    [JsonPropertyName("auto_start_proxy")]
    public bool AutoStartProxy { get; set; } = false;
    [JsonPropertyName("auto_start_zerotier")]
    public bool AutoStartZeroTier { get; set; } = false;
    [JsonPropertyName("recover_zerotier_status_on_exit")]
    public bool RecoverZeroTierStatusOnExit { get; set; } = false;
    [JsonPropertyName("network_id")]
    public string NetworkId { get; set; } = "8056c2e21c96887f";
    [JsonPropertyName("proxy_port")]
    public int ProxyPort { get; set; } = 10086;
    [JsonPropertyName("managed_devices")]
    public List<string> ManagedDevices { get; set; } = [];
    [JsonPropertyName("hostname")]
    public string Hostname = Environment.MachineName;
}