
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using Zedkiah.proxy;
using Zedkiah.zerotier;
using System.Windows.Forms; // NotifyIcon
using Color = System.Windows.Media.Color;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Zedkiah.util;
using Zedkiah.dto;
using Zedkiah.dto.zerotier.center;
using Zedkiah.zerotier.center;
using Application = System.Windows.Application;
using Timer = System.Threading.Timer;

namespace Zedkiah;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private AppConfig _appConfig;
    private NotifyIcon _notifyIcon;
    
    private readonly ServiceManager  _serviceManager = new ServiceManager();
        
    private readonly ZeroTierManager _zeroTier = new ZeroTierManager();
    
    private readonly ProxyManager _proxyManager = new ProxyManager();
    
    private readonly ZeroTierCenterManager _zeroTierCenterManager = new ZeroTierCenterManager();

    private int _zeroTierStatus = 0;
    public MainWindow()
    {
        InitializeComponent();
        SetupNotifyIcon();
        StopProcesses(["zerotier-3proxy"]);
        _appConfig = AppConfig.Load();
        StartProcess();
    }

    private void StopProcesses(string[] processNames)
    {
        foreach (var processName in processNames)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                foreach (Process p in processes)
                {
                    p.Kill();
                }
            }
        }
    }
    
    private void StartProcess()
    {
        SetAutoStartApp(_appConfig.AutoStartGui);
        CleanUpManagedDevices(_appConfig.ManagedDevices.ToArray());
        var selfInfo = _zeroTier.GetSelfInfo();
        _zeroTierCenterManager.SetNodeName(_appConfig.NetworkId,selfInfo.Address, _appConfig.Hostname);
        NetworkIdTextBox.Text = _appConfig.NetworkId;
        AutoStartAppEnabled.IsChecked = _appConfig.AutoStartGui;
        AutoStartProxyEnabled.IsChecked = _appConfig.AutoStartProxy;
        AutoStartZeroTierEnabled.IsChecked = _appConfig.AutoStartZeroTier;
        SaveConfigButton.IsEnabled = false;
        ProxyPort.Text = _appConfig.ProxyPort.ToString();
        if (_appConfig.AutoStartZeroTier || _appConfig.ZeroTierInitStatus == 2)
        {
            ClickConnectButton();
            if (_appConfig.AutoStartProxy)
            {
                Timer timer = null;
                void Callback(object? state)
                {
                    if (_zeroTierStatus == 2 && _zeroTier.networkInfo.AssignedAddresses.Count > 0 && _proxyManager.Status==0)
                    {
                        timer.DisposeAsync();
                        Application.Current.Dispatcher.Invoke(ClickProxyButton);
                    }
                }
                timer = new Timer(Callback, null, 500, 1000);
            }
        }
    }

    private void CleanUpManagedDevices(string[] devices)
    {
        using RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Profiles",true);
        var nicGuids = key.GetSubKeyNames();
        foreach (var nicGuid in nicGuids)
        {
            if (devices.Contains(nicGuid))
            {
                key.DeleteSubKey(nicGuid, false);
            }
        }
        var x = "";
    }
    private void SetupNotifyIcon()
    {
        _notifyIcon = new NotifyIcon();
        using var icon = new MemoryStream(Resource.app);
        _notifyIcon.Icon = new Icon(icon);
        _notifyIcon.Visible = true;
        _notifyIcon.Text = "ZeroTier GUI";

        // 点击托盘图标显示窗口
        _notifyIcon.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                Show();
                ShowInTaskbar = true;
                WindowState = WindowState.Normal;
                Activate();
            }
        };

        // 托盘菜单（可选）
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Exit", null, (s, e) =>
        {
            _notifyIcon.Visible = false;
            _proxyManager.Dispose();
            _zeroTier.DisConnect();
            RegUtil.DeleteInterfaceProfile(_appConfig.ManagedDevices.ToArray());
            _appConfig.ManagedDevices.Clear();
            AppConfig.Save(_appConfig);
            StopProcesses(["zerotier-3proxy"]);
            SetAutoStartApp(_appConfig.AutoStartGui);
            Application.Current.Shutdown();
        });
        _notifyIcon.ContextMenuStrip = contextMenu;
    }
    
    private void ConnectButton_OnClick(object sender, RoutedEventArgs e)
    {
        ClickConnectButton();
    }

    private string GetPeersText(List<NetworkMember> peers)
    {
        var peerText = new StringBuilder();
        peerText.AppendLine("Peers:");
        foreach (var peer in peers)
        {
            var ip = "(Unknown)";
            if (peer.Config.IpAssignments.Count > 0)
            {
                ip = string.Join(", ", peer.Config.IpAssignments);
            }

            peerText.AppendLine($"{peer.Name} ({peer.Config.Address}) : {ip}");
        }

        return peerText.ToString();
    }
    
    private Timer _refreshPeerTimer;

    private void ClickConnectButton()
    {
        _appConfig.NetworkId = NetworkIdTextBox.Text;
        
        Task.Run(() =>
        {
            _serviceManager.StartService("ZeroTierOneService");
            if (_zeroTierStatus == 0)
            {
                Dispatcher.Invoke(() =>
                {
                    ConnectButton.IsEnabled = false;
                    NetworkIdTextBox.IsEnabled = false;
                    ConnectionStatusLabel.Text = "Connecting...";
                    ConnectionStatusLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                    ProxyButton.IsEnabled = false;
                    ProxyPort.IsEnabled = false;
                    ProxyStatusLabel.Text = "No Proxy Started";
                });
                var networkInfo = _zeroTier.Connect(_appConfig.NetworkId);
                var interfaceUuid = NetworkUtil.GetInterfaceUuid(networkInfo.Mac);
                if (interfaceUuid != null)
                {
                    if (!_appConfig.ManagedDevices.Contains(interfaceUuid))
                    {
                        _appConfig.ManagedDevices.Add(interfaceUuid);
                    }
                }
                _zeroTierStatus = 2;
                Dispatcher.Invoke(() =>
                {
                    ConnectButton.Content = "Disconnect";
                    ConnectionStatusLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                    ConnectionStatusLabel.Text =
                        $"""
                         Connected to {_appConfig.NetworkId}
                         Fetching IP Address...
                         """;
                    ConnectButton.IsEnabled = true;
                });
                Timer timer = null;
                void Callback(object? state)
                {
                    if (_zeroTierStatus == 2 && _zeroTier.networkInfo.AssignedAddresses.Count > 0)
                    {
                        timer.DisposeAsync();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ProxyButton.IsEnabled = true;
                            if (_proxyManager.Status == 0)
                            {
                                ProxyPort.IsEnabled = true;
                            }
                            else
                            {
                                ProxyPort.IsEnabled = false;
                            }
                            ConnectionStatusLabel.Text =
                                $"""
                                 Connected to {_appConfig.NetworkId}
                                 Address: {_zeroTier.networkInfo.AssignedAddresses[0]}
                                 """;
                            void RefreshPeerInfoCallback(object? s)
                            {
                                RefreshPeerInfo();
                            }
                            _refreshPeerTimer = new Timer(RefreshPeerInfoCallback, null, 30500, 30500);
                            RefreshPeerInfo();
                            PeersInfoArea.Visibility = Visibility.Visible;
                        });
                    }
                }
                timer = new Timer(Callback, null, 500, 1000);
                return;
            }

            if (_zeroTierStatus == 2)
            {
                if(_refreshPeerTimer != null)
                {
                    _refreshPeerTimer.Dispose();
                    _refreshPeerTimer = null;
                }
                Dispatcher.Invoke(() =>
                {
                    ConnectButton.IsEnabled = false;
                    ConnectionStatusLabel.Text = "Disconnecting...";
                    ConnectionStatusLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                    PeersTextBox.Text = string.Empty;
                    PeersInfoArea.Visibility = Visibility.Collapsed;
                    
                    RefreshPeersButton.IsEnabled = false;
                });
                Dispatcher.Invoke(() =>
                {
                    ProxyButton.IsEnabled = false;
                });
                if (_proxyManager.Status == 2)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProxyPort.IsEnabled = false;
                        ProxyStatusLabel.Text = "Stopping...";
                    });
                    _proxyManager.StopProxy();
                }
                _zeroTier.DisConnect();
                RegUtil.DeleteInterfaceProfile(_appConfig.ManagedDevices.ToArray());
                _appConfig.ManagedDevices.Clear();
                AppConfig.Save(_appConfig);
                _zeroTierStatus = 0;
                Dispatcher.Invoke(() =>
                {
                    ConnectButton.Content = "Connect";
                    ConnectionStatusLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    ConnectionStatusLabel.Text = "Disconnected";
                    ConnectButton.IsEnabled = true;
                    NetworkIdTextBox.IsEnabled = true;
                    ProxyButton.Content = "Start Proxy";
                    ProxyStatusLabel.Text = "No Proxy Started";
                    ProxyPort.IsEnabled = true;
                });
            }
        });
    }

    private void ProxyButton_OnClick(object sender, RoutedEventArgs e)
    {
        ClickProxyButton();
    }

    private void ClickProxyButton()
    {
        if (_zeroTierStatus == 2)
        {
            if (_proxyManager.Status == 0 && _zeroTier.networkInfo.AssignedAddresses.Count > 0)
            {
                ProxyButton.IsEnabled = false;
                ProxyPort.IsEnabled = false;
                _appConfig.ProxyPort = int.Parse(ProxyPort.Text);
                var host = _zeroTier.networkInfo.AssignedAddresses[0].Split('/')[0];
                _proxyManager.StartProxy(host,_appConfig.ProxyPort);
                ProxyButton.IsEnabled = true;
                ProxyButton.Content = "Stop Proxy";
                ProxyStatusLabel.Text = $"Socks5 Proxy Started: {host}:{_appConfig.ProxyPort}";
                return;
            }

            if (_proxyManager.Status == 2)
            {
                ProxyButton.IsEnabled = false;
                _proxyManager.StopProxy();
                ProxyButton.IsEnabled = true;
                ProxyButton.Content = "Start Proxy";
                ProxyStatusLabel.Text = "No Proxy Started";
                ProxyPort.IsEnabled = true;
            }
        }
    }
    
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        ShowInTaskbar = false;
        _notifyIcon.ShowBalloonTip(2000,"Zedekiah","Application is still running", ToolTipIcon.Info);
        Hide();
    }

    private void SetAutoStartApp(bool enable)
    {
        string appName = "Zedkiah";
        string exePath = "\""+Environment.ProcessPath+"\n";
        using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
        if (enable)
        {
            if (key.GetValue(appName) != null)
            {
                if (exePath != key.GetValue(appName))
                {
                    key.DeleteValue(appName, false);
                    key.SetValue(appName, exePath);
                }
            }else
            {
                key.SetValue(appName, exePath);
            }
        }
        else
        {
            if (key.GetValue(appName) != null)
            {
                key.DeleteValue(appName, false);
            }
        }
    }

    private void SaveConfigButton_OnClick(object sender, RoutedEventArgs e)
    {
        _appConfig.AutoStartGui = AutoStartAppEnabled.IsChecked== true;
        _appConfig.AutoStartZeroTier = AutoStartZeroTierEnabled.IsChecked == true;
        _appConfig.AutoStartProxy = AutoStartProxyEnabled.IsChecked == true;
        SaveConfigButton.IsEnabled = false;
        SaveConfigButton.Content = "Saving...";
        AppConfig.Save(_appConfig);
        Task.Delay(1000).ContinueWith(_ =>
        {
            Dispatcher.Invoke(() =>
            {
                SaveConfigButton.Content = "Saved";
            });
        });
        Task.Delay(2000).ContinueWith(_ =>
        {
            Dispatcher.Invoke(() =>
            {
                SaveConfigButton.Content = "Save";
            });
        });
    }

    private void AppConfig_OnChange(object sender, RoutedEventArgs e)
    {
        SaveConfigButton.Content = "Save";
        SaveConfigButton.IsEnabled = true;
    }

    private void SocksPort_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (ProxyPort.Text.Length == 0)
        {
            return;
        }
        var oldText = ProxyPort.Text[..^1];

        if (int.TryParse(ProxyPort.Text, out int value))
        {
            if (value < 1 || value > 65535)
            {
                ProxyPort.Text = oldText;
            }
        }
        else
        {
            ProxyPort.Text = oldText;
        }
    }

    private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = false;
        e.Handled = true;
    }

    private void SocksPort_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var textBox = sender as System.Windows.Controls.TextBox;

        // 获取输入的文本
        string inputText = e.Text;

        // 预测改变后的文本内容
        string futureText = textBox.Text.Insert(textBox.CaretIndex, inputText);

        if (int.TryParse(futureText, out int value))
        {
            if (value < 1 || value > 65535)
            {
                e.Handled = true;
            }
        }
        else
        {
            e.Handled = true;
        }
        
    }

    private void RefreshPeersButton_OnClick(object sender, RoutedEventArgs e)
    {
        Task.Run(RefreshPeerInfo);
    }

    private void RefreshPeerInfo()
    {
        Dispatcher.Invoke(() =>
        {
            RefreshPeersButton.Content = "Refreshing...";
            RefreshPeersButton.IsEnabled = false;
            PeersInfoArea.Visibility = Visibility.Visible;
        });
        var onlinePeers = _zeroTier.GetPeers().FindAll((peer) =>
        {
            if ("LEAF" == peer.Role)
            {
                return true;
            }
            return false;
        }).Select(peer=>peer.Address).ToList();
        onlinePeers.Insert(0, _zeroTier.GetSelfInfo().Address);
        var peers = _zeroTierCenterManager.GetNetworkMembers(_appConfig.NetworkId,onlinePeers);
        Dispatcher.Invoke(() =>
        {
            PeersTextBox.Text = GetPeersText(peers);
            RefreshPeersButton.Content = "Refresh";
            RefreshPeersButton.IsEnabled = true;
        });
    }
}