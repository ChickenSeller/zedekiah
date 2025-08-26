using Microsoft.Win32;
using Zedkiah.dto.zerotier;

namespace Zedkiah.util;

public class NetworkUtil
{
    public static string? GetInterfaceUuid(string macAddress)
    {
        var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
        foreach (var ni in interfaces)
        {
            var tmpMac = ni.GetPhysicalAddress().ToString();
            if (tmpMac == macAddress.Replace(":", "").ToUpper())
            {
                return ni.Id;
            }
        }
        return null;
    }

    
}