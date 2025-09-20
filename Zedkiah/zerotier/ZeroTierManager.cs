using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text.Json;
using Zedkiah.dto.zerotier;

namespace Zedkiah.zerotier;

public class ZeroTierManager
{
    private static string token = File.ReadAllText(@"C:\ProgramData\ZeroTier\One\authtoken.secret").Trim();

    private static readonly string BaseUrl = "http://127.0.0.1:9993";

    private string connectedNetworkId = "";
    
    private Timer _checkStatusTimer;
    
    public NetworkInfo networkInfo { get; private set; } = null;

    public SelfInfo GetSelfInfo()
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(BaseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 加入网络
        var response = client.GetAsync("status").Result;
        return JsonSerializer.Deserialize<SelfInfo>(response.Content.ReadAsStringAsync().Result);
    }

    public List<ZeroTierPeerInfo> GetPeers()
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(BaseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = client.GetAsync("peer").Result;
        return JsonSerializer.Deserialize<List<ZeroTierPeerInfo>>(response.Content.ReadAsStringAsync().Result);
    }
    
    public NetworkInfo Connect(string networkId)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(BaseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 加入网络
        var response = client.PostAsync($"network/{networkId}", null).Result;
        connectedNetworkId = networkId;
        void Callback(object? state) => CheckStatus();
        _checkStatusTimer = new Timer(Callback, null, 500, 500);
        networkInfo = JsonSerializer.Deserialize<NetworkInfo>(response.Content.ReadAsStringAsync().Result);
        return networkInfo;
    }

    public bool DisConnect()
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(BaseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = client.DeleteAsync($"network/{connectedNetworkId}");
        if (!response.Result.IsSuccessStatusCode)
        {
            connectedNetworkId = "";
            return false;
        }
        var result = response.Result.Content.ReadAsStringAsync().Result;
        return true;
    }

    public static int GetInitStatus(string networkId)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(BaseUrl);
        client.Timeout = new TimeSpan(0, 0, 1);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            var response = client.GetAsync($"network/{networkId}").Result;
            if (!response.IsSuccessStatusCode)
            {
                return 1;
            }

            return 2;
        }
        catch (Exception)
        {
            return 0;
        }
        
    }
    
    private void CheckStatus()
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(BaseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 加入网络
        var response = client.GetAsync($"network/{connectedNetworkId}").Result;
        var result = JsonSerializer.Deserialize<NetworkInfo>(response.Content.ReadAsStringAsync().Result);
        if(result.Status == "OK")
        {
            _checkStatusTimer.Dispose();
            using var ping = new Ping();
            for (;;)
            {
                var pingStatus = ping.Send(result.AssignedAddresses.First().Split("/")[0], 10000).Status;
                if (pingStatus == IPStatus.Success)
                {
                    networkInfo = result;
                    return;
                }
            }
        }
    }
}