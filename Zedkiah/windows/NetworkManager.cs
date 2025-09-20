using System.Net.NetworkInformation;

namespace Zedkiah.windows;

public class NetworkManager
{
    private static Dictionary<string,NetworkInterface> interfaces;
    public static void GetDifference()
    {
        var addedInterfaces = new List<NetworkInterface>();
        var currentInterfaces = GetAllNetworkInterfaces();
        foreach (var networkInterface in currentInterfaces)
        {
            currentInterfaces.ExceptBy(interfaces, ni => ni);
        }
        interfaces = currentInterfaces;
    }
    
    public static void GetInitialInterfaces()
    {
        interfaces = GetAllNetworkInterfaces();
    }
    public static Dictionary<string,NetworkInterface> GetAllNetworkInterfaces()
    {
        List<string> interfaces = new List<string>();
        var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
        foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            interfaces.Add(ni.Name);
        }
        return networkInterfaces.ToDictionary(ni => ni.Id, ni => ni);
    }
}