using Zedkiah.consts;

namespace Zedkiah.dto.service;

public class DetectedHostInfo
{
    public string NodeId { get; set; }
    public string HostName { get; set; }
    public string[] IpAddresses { get; set; } = [];
    public HostDetectMethod[] Methods { get; set; }
    public HostDetectMethod PrimaryMethod { get; set; }
    public HostInfo? RawInfo { get; set; }
}