using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Zedkiah.dto;
using Zedkiah.dto.service;
using Zedkiah.util;

namespace Zedkiah.service;

public class UdpService
{
    private Thread _workerThread;
    private UdpClient _udp;
    private static int _port = 17958;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    
    public void StartServer()
    {
        
        _workerThread = new Thread(() =>
            {
                _udp = new UdpClient();
                _udp.Client.Bind(new IPEndPoint(IPAddress.Any, _port));
                _udp.EnableBroadcast = true;

                Console.WriteLine($"UDP Broadcast Server listening on port {_port} ...");

                CancellationToken token = _cts.Token;

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (_udp.Available > 0)
                        {
                            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                            try
                            {
                                byte[] data = _udp.Receive(ref remote);
                                var query = JsonSerializer.Deserialize<UdpQuery>(Encoding.UTF8.GetString(data));
                                if (query != null && query.Type == "query_info")
                                {
                                    if (remote.Address.ToString() ==
                                        NetworkUtil.GetHostAddress(EnvironmentInfo.IpAddress))
                                    {
                                        continue;
                                    }
                                    var response = new UdpResponse();
                                    response.Type = "zedekiah_info";
                                    var content = new UdpTransferHostInfo();
                                    response.Content = content;
                                    content.IpAddress = EnvironmentInfo.IpAddress;
                                    content.NodeId = EnvironmentInfo.SelfInfo.NodeId;
                                    content.HostName = AppConfig.Config.Hostname;
                                    if (EnvironmentInfo.ProxyEnabled)
                                    {
                                        content.ProxyPort = EnvironmentInfo.ProxyPort;
                                    }
                                    byte[] responseData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                                    _udp.Send(responseData, responseData.Length, remote);
                                }
                            }catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
                            {
                                Console.WriteLine("Ignore ICMP 10054");
                            }
                        }
                        else
                        {
                            Thread.Sleep(50);
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    // udp 已关闭，线程安全退出
                }
                finally
                {
                    _udp?.Close();
                    Console.WriteLine("UDP server stopped.");
                }
            });
        _workerThread.IsBackground = true;
        _workerThread.Start();
    }
    
    public void StopServer()
    {
        _cts.Cancel();
        _udp?.Close();
        _workerThread?.Join();
    }

    public static Dictionary<string,UdpResponse> QueryPeerInfo()
    {
        string serverIp = NetworkUtil.GetBroadcastAddressString(EnvironmentInfo.IpAddress);
        int serverPort = _port;
        int timeoutMs = 5000;
        Dictionary<string,UdpResponse> result = new Dictionary<string, UdpResponse>();
        using (UdpClient client = new UdpClient())
        {
            client.EnableBroadcast = true;
            string message = "Hello UDP Server!";
            byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new UdpQuery { Type = "query_info" }));
            client.Send(data, data.Length, serverIp, serverPort);
            Console.WriteLine($"Sent: {message}");
            client.Client.ReceiveTimeout = timeoutMs;

            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    var x = Encoding.UTF8.GetString(client.Receive(ref remoteEP));
                    // var response = JsonSerializer.Deserialize<UdpResponse>(Encoding.UTF8.GetString(client.Receive(ref remoteEP)));
                    // result[remoteEP.Address.ToString()] = response;
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    break;
                }
            }
        }
        return result;
    }
    
}