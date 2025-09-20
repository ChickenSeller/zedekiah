namespace Zedkiah.dto;

public class PeerInfo
{
    public string Id { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public int ProxyPort { get; set; }
    public string ClientType { get; set; } = "ZeroTier";
}