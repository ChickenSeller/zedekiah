using System.Diagnostics;
using System.IO;

namespace Zedkiah.proxy;

public class ProxyManager
{
    
    private Process _proxyProcess;
    
    public int Status { get; private set; } = 0;
    
    public void Dispose()
    {
        StopProxy();
        _proxyProcess?.Dispose();
    }
    public void StartProxy(string host, int port)
    {
        _proxyProcess = Start3Proxy(host, port);
        Status = 2;
    }

    public void StopProxy(bool clearLogs = false)
    {
        if (_proxyProcess is {HasExited: false})
        {
            _proxyProcess.Kill();
            _proxyProcess.WaitForExit();
        }

        Delete3ProxyFiles(clearLogs);
        Status = 0;
    }

    private Process Start3Proxy(string host, int port)
    {
        var basePath = AppContext.BaseDirectory;
        if (!basePath.EndsWith("\\"))
        {
            basePath += "\\";
        }
        var proxyPath = basePath + "proxy\\bin64\\zerotier-3proxy.exe";
        var cfgPath = basePath + "proxy\\proxy.cfg";
        if (File.Exists(cfgPath))
        {
            File.Delete(cfgPath);
        }
        var logDir = basePath + "proxy\\logs";
        var proxyCfgContent = 
            $"""
            #!/usr/local/bin/3proxy
            system "echo 3proxy up!"
            timeouts 1 5 30 60 180 1800 15 60
            service
            nserver 1.1.1.1
            nserver 8.8.8.8
            nscache 65536
            nscache6 65535
            log "{logDir}\%Y%m%d.log" D
            logformat "- +_L%t.%. %Y-%m-%d  %N.%p %E %U %C:%c %R:%r %O %I %h %T"
            archiver rar rar a -df -inul %A %F
            rotate 30
            internal {host}
            auth none
            dnspr
            auth none
            flush
            allow *
            maxconn 20
            socks
            socks -p{port}
            """;
        File.WriteAllText(cfgPath, proxyCfgContent);
        var psi = new ProcessStartInfo
        {
            FileName = proxyPath,
            Arguments = cfgPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        _proxyProcess = Process.Start(psi);
        return _proxyProcess;
    }

    private void Delete3ProxyFiles(bool clearLogs)
    {
        var basePath = AppContext.BaseDirectory;
        if (!basePath.EndsWith("\\"))
        {
            basePath += "\\";
        }
        var cfgPath = basePath + "proxy\\proxy.cfg";
        if (File.Exists(cfgPath))
        {
            File.Delete(cfgPath);
        }

        if (clearLogs)
        {
            var logDir = basePath + "proxy\\logs";
            if (Directory.Exists(logDir))
            {
                var logFiles = Directory.GetFiles(logDir);
                foreach (var file in logFiles)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }
    }
}