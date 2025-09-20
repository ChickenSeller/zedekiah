using System.Net;
using Zedkiah.dto.zerotier;

namespace Zedkiah;

public class EnvironmentInfo
{
    public static string IpAddress { get; set; }
    
    public static bool ProxyEnabled { get; set; }
    public static int ProxyPort { get; set; }
    public static SelfInfo SelfInfo { get; set; }
    public static string NetworkId { get; set; }
}