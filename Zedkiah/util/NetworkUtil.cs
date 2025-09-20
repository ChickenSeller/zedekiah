using System.Net;

namespace Zedkiah.util;

public class NetworkUtil
{
    public static string GetBroadcastAddressString(string cidr)
    {
        var parts = cidr.Split('/');
        if (parts.Length != 2)
            throw new FormatException("CIDR 格式不正确");

        IPAddress ip = IPAddress.Parse(parts[0]);
        int prefixLength = int.Parse(parts[1]);

        if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            throw new NotSupportedException("仅支持 IPv4");

        uint ipUint = BitConverter.ToUInt32(ip.GetAddressBytes().Reverse().ToArray(), 0);

        uint mask = uint.MaxValue << (32 - prefixLength);

        uint broadcastUint = (ipUint & mask) | ~mask;

        return new IPAddress(BitConverter.GetBytes(broadcastUint).Reverse().ToArray()).ToString();
    }

    public static string GetHostAddress(string cidr)
    {
        var parts = cidr.Split('/');
        if (parts.Length != 2)
            throw new FormatException("CIDR 格式不正确");

        var ip = IPAddress.Parse(parts[0]);
        
        if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            throw new NotSupportedException("仅支持 IPv4");
        
        return ip.ToString();
        
        
    }
}