using Microsoft.Win32;

namespace Zedkiah.util;

public class RegUtil
{
    public static DateTime? GetFileTime(byte[] fileTimeBytes)
    {
        if (fileTimeBytes == null || fileTimeBytes.Length != 8)
            return null;

        long ft = BitConverter.ToInt64(fileTimeBytes, 0);
        return DateTime.FromFileTimeUtc(ft);
    }

    public static void DeleteInterfaceProfile(string[] guids)
    {
        using RegistryKey profiles =
            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Profiles\", true);
        if (profiles != null)
        {
            using RegistryKey signatures = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Signatures\Unmanaged\", true);;
            foreach (var guid in guids)
            {
                foreach (var signatureKey in signatures?.GetSubKeyNames())
                {
                    ParseNetworkSignature(signatureKey);
                }
                
                // profiles.DeleteSubKeyTree(guid,true);
            }
            profiles.Close();
        }
    }
    
    public static byte[] HexStringToBytes(string hex)
    {
        if (hex.Length % 2 != 0)
            throw new ArgumentException("Hex string length must be even.");

        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }
    
    public static void ParseNetworkSignature(string signatureStr)
    {
        var bytes = HexStringToBytes(signatureStr);
        Console.WriteLine($"Raw Length: {bytes.Length} bytes");

        // 前 4 字节：Version/Type
        uint version = BitConverter.ToUInt32(bytes.Take(4).ToArray());
        Console.WriteLine($"Version/Type : {version}");

        // 第 4-7 字节：Flags/Length
        uint flags = BitConverter.ToUInt32(bytes.Skip(4).Take(16).ToArray());
        Console.WriteLine($"Flags/Length : {flags}");

        // 偏移 8-23：尝试 GUID
        if (bytes.Length >= 24)
        {
            try
            {
                Guid guid1 = new Guid(bytes.Skip(8).Take(16).ToArray());
                Console.WriteLine($"GUID1        : {guid1}");
            }
            catch
            {
                Console.WriteLine("GUID1        : <Invalid>");
            }
        }

        // 偏移 24-39：尝试 GUID
        if (bytes.Length >= 40)
        {
            try
            {
                Guid guid2 = new Guid(bytes.Skip(24).Take(16).ToArray());
                Console.WriteLine($"GUID2        : {guid2}");
            }
            catch
            {
                Console.WriteLine("GUID2        : <Invalid>");
            }
        }

        // 偏移 24-29：尝试 MAC
        if (bytes.Length >= 30)
        {
            string mac = string.Join("-", bytes.Skip(24).Take(6).Select(b => b.ToString("X2")));
            Console.WriteLine($"Maybe MAC    : {mac}");
        }
    }
}