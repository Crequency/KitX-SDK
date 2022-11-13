using KitX.Web.Rules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable CS8604 // 引用类型参数可能为 null。

namespace KitX.Device.Simulator
{

    public class Program
    {

        public static void Main(string[] args)
        {
            new Thread(() =>
            {
                FindSurpportNetworkInterface(new()
                    {
                        UdpClient_Send, UdpClient_Receive
                    }, IPAddress.Parse("224.0.0.0"));
                MultiDevicesBroadCastSend();
                MultiDevicesBroadCastReceive();
            }).Start();
        }

        #region UDP Socket 服务于自发现自组网

        private static readonly List<int> SurpportedNetworkInterfaces = new();

        internal static DeviceInfoStruct DefaultDeviceInfoStruct = GetDeviceInfo();

        /// <summary>
        /// UDP 发包客户端
        /// </summary>
        private static readonly UdpClient UdpClient_Send
            = new(23404, AddressFamily.InterNetwork)
            {
                EnableBroadcast = true,
                MulticastLoopback = true
            };

        /// <summary>
        /// UDP 收包客户端
        /// </summary>
        private static readonly UdpClient UdpClient_Receive
            = new(new IPEndPoint(IPAddress.Any, 24040));

        /// <summary>
        /// 寻找受支持的网络适配器并把UDP客户端加入组播
        /// </summary>
        private static void FindSurpportNetworkInterface(List<UdpClient> clients, IPAddress multicastAddress)
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                IPInterfaceProperties ip_properties = adapter.GetIPProperties();
                if (adapter.GetIPProperties().MulticastAddresses.Count == 0
                    // most of VPN adapters will be skipped
                    || !adapter.SupportsMulticast
                    // multicast is meaningless for this type of connection
                    || OperationalStatus.Up != adapter.OperationalStatus
                    // this adapter is off or not connected
                    || !adapter.Supports(NetworkInterfaceComponent.IPv4)
                    ) continue;
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                UnicastIPAddressInformationCollection unicastIPAddresses
                    = adapterProperties.UnicastAddresses;
                IPv4InterfaceProperties p = adapterProperties.GetIPv4Properties();
                if (p == null) continue;    // IPv4 is not configured on this adapter
                SurpportedNetworkInterfaces.Add(IPAddress.HostToNetworkOrder(p.Index));
                IPAddress ipAddress = null;
                foreach (UnicastIPAddressInformation unicastIPAddress in unicastIPAddresses)
                {
                    if (unicastIPAddress.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                    ipAddress = unicastIPAddress.Address;
                    break;
                }
                if (ipAddress == null) continue;
                foreach (var udpClient in clients)
                    udpClient.JoinMulticastGroup(multicastAddress, ipAddress);
            }
        }

        /// <summary>
        /// 多设备广播发送方法
        /// </summary>
        public static void MultiDevicesBroadCastSend()
        {
            #region 初始化 UDP 客户端

            UdpClient udpClient = UdpClient_Send;
            IPEndPoint multicast = new(IPAddress.Parse("224.0.0.0"),
                24040);
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress, true);

            #endregion

            System.Timers.Timer timer = new()
            {
                Interval = 1000,
                AutoReset = true
            };
            timer.Elapsed += (_, _) =>
            {
                UpdateDefaultDeviceInfoStruct();
                string sendText = JsonSerializer.Serialize(DefaultDeviceInfoStruct);
                byte[] sendBytes = Encoding.UTF8.GetBytes(sendText);

                foreach (var item in SurpportedNetworkInterfaces)
                {
                    udpClient.Client.SetSocketOption(SocketOptionLevel.IP,
                        SocketOptionName.MulticastInterface, item);
                    udpClient.Send(sendBytes, sendBytes.Length, multicast);
                }
                if (!true)
                {
                    udpClient.Close();

                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        /// <summary>
        /// 多设备广播接收方法
        /// </summary>
        public static void MultiDevicesBroadCastReceive()
        {
            #region 初始化 UDP 客户端

            UdpClient udpClient = UdpClient_Receive;
            IPEndPoint multicast = new(IPAddress.Any, 0);
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress, true);

            #endregion

            new Thread(() =>
            {
                while (true)
                {
                    byte[] bytes = udpClient.Receive(ref multicast);
                    string result = Encoding.UTF8.GetString(bytes);

                    DeviceInfoStruct deviceInfo = JsonSerializer.Deserialize<DeviceInfoStruct>(result);
                    Console.WriteLine($"Receive: {result}");
                    // DevicesManager.Update(deviceInfo);
                }
                udpClient.Close();
                if (!true)
                {
                    udpClient.Close();
                }
                else
                {
                    MultiDevicesBroadCastReceive();
                }
            }).Start();
        }

        /// <summary>
        /// 将 IPv4 的十进制表示按点分制拆分
        /// </summary>
        /// <param name="ip">IPv4 的十进制表示</param>
        /// <returns>拆分</returns>
        private static (int, int, int, int) IPv4_2_4Parts(string ip)
        {
            string[] p = ip.Split('.');
            int a = int.Parse(p[0]), b = int.Parse(p[1]), c = int.Parse(p[2]), d = int.Parse(p[3]);
            return (a, b, c, d);
        }

        /// <summary>
        /// 获取本机内网 IPv4 地址
        /// </summary>
        /// <returns>使用点分十进制表示法的本机内网IPv4地址</returns>
        private static string GetInterNetworkIPv4()
        {
            return (from ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList
                    where ip.AddressFamily == AddressFamily.InterNetwork
                        && !ip.ToString().Equals("127.0.0.1")
                        && (ip.ToString().StartsWith("192.168")                         //  192.168.x.x
                            || ip.ToString().StartsWith("10")                           //  10.x.x.x
                            || (IPv4_2_4Parts(ip.ToString()).Item1 == 172               //  172.16-31.x.x
                                && IPv4_2_4Parts(ip.ToString()).Item2 >= 16
                                && IPv4_2_4Parts(ip.ToString()).Item2 <= 31))
                        && ip.ToString().StartsWith("192.168")  //  满足自定义规则
                    select ip).First().ToString();
        }

        /// <summary>
        /// 获取设备信息
        /// </summary>
        /// <returns>设备信息结构体</returns>
        private static DeviceInfoStruct GetDeviceInfo() => new()
        {
            DeviceName = Environment.MachineName,
            DeviceMacAddress = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up
                    && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault(),
            IsMainDevice = false,
            SendTime = DateTime.Now,
            DeviceOSType = OperatingSystem2Enum.GetOSType(),
            DeviceOSVersion = Environment.OSVersion.VersionString,
            IPv4 = GetInterNetworkIPv4(),
            IPv6 = (from ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList
                    where ip.AddressFamily == AddressFamily.InterNetworkV6
                        && !ip.ToString().Equals("::1")
                    select ip).First().ToString(),
            ServingPort = 2000,
            ServerAddress = "",
            ServerBuildTime = new(),
            PluginsCount = 0,
        };

        /// <summary>
        /// 更新默认设备信息结构
        /// </summary>
        private static void UpdateDefaultDeviceInfoStruct()
        {
            DefaultDeviceInfoStruct.IsMainDevice = false;
            DefaultDeviceInfoStruct.SendTime = DateTime.Now;
            DefaultDeviceInfoStruct.DeviceOSVersion = Environment.OSVersion.VersionString;
            DefaultDeviceInfoStruct.IPv4 = GetInterNetworkIPv4();
            DefaultDeviceInfoStruct.IPv6 = (from ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList
                                            where ip.AddressFamily == AddressFamily.InterNetworkV6
                                                && !ip.ToString().Equals("::1")
                                            select ip).First().ToString();
            DefaultDeviceInfoStruct.ServingPort = 10000;
            DefaultDeviceInfoStruct.PluginsCount = 0;
            DefaultDeviceInfoStruct.IsMainDevice = false;
            DefaultDeviceInfoStruct.ServerAddress = "";
            DefaultDeviceInfoStruct.ServerBuildTime = DateTime.Now;
        }

        #endregion
    }

    internal class OperatingSystem2Enum
    {
        internal static OperatingSystems GetOSType()
        {
            return OperatingSystems.IoT;

            if (OperatingSystem.IsAndroid())
                return OperatingSystems.Android;
            if (OperatingSystem.IsBrowser())
                return OperatingSystems.Browser;
            if (OperatingSystem.IsFreeBSD())
                return OperatingSystems.FreeBSD;
            if (OperatingSystem.IsIOS())
                return OperatingSystems.IOS;
            if (OperatingSystem.IsLinux())
                return OperatingSystems.Linux;
            if (OperatingSystem.IsMacCatalyst())
                return OperatingSystems.MacCatalyst;
            if (OperatingSystem.IsMacOS())
                return OperatingSystems.MacOS;
            if (OperatingSystem.IsTvOS())
                return OperatingSystems.TvOS;
            if (OperatingSystem.IsWatchOS())
                return OperatingSystems.WatchOS;
            if (OperatingSystem.IsWindows())
                return OperatingSystems.Windows;

            return OperatingSystems.Unknown;
        }
    }
}

#pragma warning restore CS8604 // 引用类型参数可能为 null。
#pragma warning restore CS8602 // 解引用可能出现空引用。
#pragma warning restore CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
