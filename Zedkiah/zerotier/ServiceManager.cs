using System.ServiceProcess;

namespace Zedkiah.zerotier;

public class ServiceManager
{
    private const int TimeoutMilliseconds = 15000;
    
    private const string _serviceName = "ZeroTierOneService";

    public bool StartService()
    {
        TimeSpan timeout = TimeSpan.FromMilliseconds(TimeoutMilliseconds);
        using (ServiceController service = new ServiceController(_serviceName))
        {
            if (service.Status == ServiceControllerStatus.Running)
            {
                Console.WriteLine($"服务 '{_serviceName}' 已经在运行中");
                return true;
            }
            
            if (service.Status == ServiceControllerStatus.Paused)
            {
                service.Continue();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                Console.WriteLine($"服务 '{_serviceName}' 已恢复运行");
                return true;
            }

            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            Console.WriteLine($"服务 '{_serviceName}' 启动成功");
            return true;
        }
    }

    public bool StopService()
    {
        TimeSpan timeout = TimeSpan.FromMilliseconds(TimeoutMilliseconds);
        using (ServiceController service = new ServiceController(_serviceName))
        {
            if (service.Status == ServiceControllerStatus.Stopped)
            {
                Console.WriteLine($"服务 '{_serviceName}' 已经停止");
                return true;
            }
            if (!service.CanStop)
            {
                Console.WriteLine($"服务 '{_serviceName}' 无法停止");
                return false;
            }
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            Console.WriteLine($"服务 '{_serviceName}' 停止成功");
            return true;
        }
    }
    
    public ServiceControllerStatus GetServiceStatus()
    {
        using (ServiceController service = new ServiceController(_serviceName))
        {
            return service.Status;
        }
    }
}