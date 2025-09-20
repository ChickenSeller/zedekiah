using System.Net.Http;
using Zedkiah.dto;
using Zedkiah.dto.zerotier.center;

namespace Zedkiah.zerotier.center;

public class ZeroTierCenterManager
{
    
    private const string BaseUrl = "https://my.zerotier.com/api/v1/";
    
    public void SetNodeName()
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(BaseUrl);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ZeroTierCenterConfig.Config.ApiKey);
        
        var content = new StringContent($"{{\"name\":\"{AppConfig.Config.Hostname}\"}}", System.Text.Encoding.UTF8, "application/json");
        var response = client.PostAsync($"network/{EnvironmentInfo.NetworkId}/member/{EnvironmentInfo.SelfInfo.NodeId}", content).Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to set node name: {response.Content.ReadAsStringAsync().Result}");
        }
    }
    
    public List<NetworkMember> GetNetworkMembers(string networkId,List<string> onlineNodeIds)
    {
        var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        using var client = new HttpClient();
        client.BaseAddress = new Uri(BaseUrl);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ZeroTierCenterConfig.Config.ApiKey);
        
        var response = client.GetAsync($"network/{networkId}/member").Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get network members: {response.Content.ReadAsStringAsync().Result}");
        }
        
        var json = response.Content.ReadAsStringAsync().Result;
        var rawResult = System.Text.Json.JsonSerializer.Deserialize<List<NetworkMember>>(json) ?? [];
        if(onlineNodeIds.Count == 0)
        {
            return rawResult;
        }
        var result = rawResult.FindAll((result) => onlineNodeIds.Contains(result.NodeId));
        return result;
    }
}