using System.Net;
using System.Net.Sockets;
using Makaretu.Dns;
using Zedkiah.consts;
using Zedkiah.dto.service;
using Zedkiah.dto.zerotier.center;
using Zedkiah.zerotier;
using Zedkiah.zerotier.center;

namespace Zedkiah.service;

public class HostDetectService
{
    public ZeroTierManager? ZeroTier{get;set;}
    public ZeroTierCenterManager? ZeroTierCenterManager{get;set;}
    
    public string NetworkId{get;set;}
    
    public List<DetectedHostInfo> GetHosts(params HostDetectMethod [] methods)
    {
        
        if (methods.Length == 0)
        {
            methods = [HostDetectMethod.ZedekiahPeer];
        }

        
        
        // Detect with ZeroTier Local API
        List<string> localPeers = new List<string>();
        if (methods.Contains(HostDetectMethod.ZeroTierLocal))
        {
            localPeers = ZeroTier.GetPeers().FindAll((peer) =>
            {
                if ("LEAF" == peer.Role)
                {
                    return true;
                }
                return false;
            }).Select(peer=>peer.Address).ToList();
            var selfInfo = ZeroTier.GetSelfInfo();
            localPeers.Insert(0, selfInfo.NodeId);
        }
        
        // Detect with ZeroTier Central API
        List<NetworkMember> zeroTierCentralMembers = [];
        if (methods.Contains(HostDetectMethod.ZeroTierCenter) && ZeroTierCenterManager != null)
        {
            zeroTierCentralMembers = ZeroTierCenterManager.GetNetworkMembers(NetworkId, []);
        }
        
        // Detect Zedekiah Peers
        if (methods.Contains(HostDetectMethod.ZedekiahPeer))
        {
            var x = UdpService.QueryPeerInfo();
            Console.WriteLine(x);
        }

        // Detect with ZeroConf (mDNS)
        if (methods.Contains(HostDetectMethod.ZeroConf))
        {
            var mdns = new MulticastService();
            var sd = new ServiceDiscovery(mdns);
            sd.ServiceInstanceDiscovered += (s, e) =>
            {
                Console.WriteLine($"service instance '{e.ServiceInstanceName}'");

                // Ask for the service instance details.
                mdns.SendQuery(e.ServiceInstanceName, type: DnsType.SRV);
            };

            mdns.AnswerReceived += (s, e) =>
            {
                if (!e.RemoteEndPoint.Address.ToString().StartsWith("10.241."))
                {
                    return;
                }
                // Is this an answer to a service instance details?
                var servers = e.Message.Answers.OfType<SRVRecord>();
                foreach (var server in servers)
                {
                    Console.WriteLine($"host '{server.Target}' for '{server.Name}'");

                    // Ask for the host IP addresses.
                    mdns.SendQuery(server.Target, type: DnsType.A);
                    //mdns.SendQuery(server.Target, type: DnsType.AAAA);
                }

                // Is this an answer to host addresses?
                var addresses = e.Message.Answers.OfType<AddressRecord>();
                foreach (var address in addresses)
                {
                    if (address.Address.AddressFamily== AddressFamily.InterNetwork)
                        Console.WriteLine($"host '{address.Name}' at {address.Address}");
                }
                // Get connectionstring from DNS TXT record.
                var txts = e.Message.Answers.OfType<TXTRecord>();
                foreach (var txt in txts)
                {
                    //“connstr=Server”，获得对应connstr值
                    Console.WriteLine($"{txt.Strings.Single(w => w.Contains("connstr")).Split('=')[1]}");
                    //Console.WriteLine($"host '{address.Name}' at {address.Address}");
                }
            };

            try
            {
                mdns.Start();
                sd.QueryServiceInstances("_workstation._tcp.local");
            }
            finally
            {
                // sd.Dispose();
                // mdns.Stop();
            }
        }
        
        var hosts = new List<DetectedHostInfo>();
        var controllerId = "";
        localPeers.ForEach(localPeer =>
        {
            NetworkMember networkMember = zeroTierCentralMembers.Find(zeroTierCentralMember => zeroTierCentralMember.NodeId == localPeer);
            List<string> ipAddresses = [];
            ipAddresses.AddRange(networkMember?.Config.IpAssignments??["(Unknown)"]);
            controllerId = networkMember?.ControllerId??"";
            hosts.Add(new DetectedHostInfo
            {
                NodeId = networkMember?.NodeId?? localPeer,
                HostName = networkMember?.Name ?? localPeer,
                IpAddresses = ipAddresses.ToArray(),
                Methods = [HostDetectMethod.ZeroTierLocal, HostDetectMethod.ZeroTierCenter],
                RawInfo = networkMember,
            });
        });

        localPeers = localPeers.Where(localPeer =>
            zeroTierCentralMembers.All(zeroTierCentralMember => zeroTierCentralMember.NodeId != localPeer)).ToList();

        return hosts.Where(host=>host.NodeId != controllerId).ToList();
    }
}