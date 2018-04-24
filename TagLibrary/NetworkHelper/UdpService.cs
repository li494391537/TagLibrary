using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Lirui.TagLibrary.NetworkHelper {
    class UdpService {
        public static int HttpPort { get; set; }
        public static int Port => port;
        private static readonly int port = 4456;
        private static UdpClient udpClient;
        private static System.Timers.Timer timer = new System.Timers.Timer(10000);
        private static IEnumerable<IPAddress> BroadcastAddress;
        private static IEnumerable<(IPAddress IP, IPAddress Mask)> iPAddresses;
        private static Task task;
        private static CancellationTokenSource cancellationTokenSource;

        private static List<HostInfo> hostInfos = new List<HostInfo>();

        public static List<HostInfo> HostInfos { get => hostInfos; }


        static UdpService() {
            iPAddresses =
                NetworkInterface.GetAllNetworkInterfaces()
                //取出可以使用以及支持多播的网络接口
                .Where(item => item.OperationalStatus == OperationalStatus.Up && item.SupportsMulticast == true)
                //查出所有接口上的所有IP
                .SelectMany(item => item.GetIPProperties().UnicastAddresses)
                //取出IPv4地址
                .Where(item => item.Address.AddressFamily == AddressFamily.InterNetwork)
                //取出地址和掩码
                .Select(item => {
                    (IPAddress IP, IPAddress Mask) r = (item.Address, item.IPv4Mask);
                    return r;
                })
                //取出非Loopback地址
                .Where(item => !IPAddress.IsLoopback(item.IP));

            BroadcastAddress =
                iPAddresses
                //计算广播地址
                .Select(item => {
                    var ip = item.IP.GetAddressBytes();
                    var mask = item.Mask.GetAddressBytes();
                    byte[] buffer = new byte[4];
                    for (int i = 0; i < 4; i++) {
                        buffer[i] = (byte) (ip[i] | (~mask[i]));
                    }
                    return new IPAddress(buffer);
                });

            cancellationTokenSource = new CancellationTokenSource();
            task = new Task(Receive, cancellationTokenSource.Token);
        }


        public static bool StartSendHeartBeat() {
            try {
                udpClient = udpClient ?? new UdpClient(port);
                timer.Elapsed += SendHeartBeat;
                timer.Start();
                return true;
            } catch { return false; }
        }

        public static void StopSendHeartBeat() {
            timer.Stop();
            timer.Elapsed -= SendHeartBeat;
        }

        public static bool StartReceive() {
            try {
                if (udpClient == null) {
                    udpClient = new UdpClient(port);
                }
                task.Start();
                return true;
            } catch { return false; }
        }

        public static void StopReceive() {
            cancellationTokenSource.Cancel();
        }

        private static void SendHeartBeat(object sender, ElapsedEventArgs e) {
            SendHeartBeat();
        }

        public static void SendHeartBeat() {
            //向广播列表内每一项广播心跳包
            foreach (var (ip, mask) in iPAddresses) {
                Send(ip, mask);
            }
        }

        private static void Receive() {
            while (true) {
                try {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                    var data = udpClient.Receive(ref endPoint);
                    if (data.Length != 8) continue;
                    if (!(data[0] == 0x10 && data[1] == 0x11 && data[2] == 0x22 && data[3] == 0x23)) continue;
                    byte[] buffer = new byte[4];
                    Array.Copy(data, 4, buffer, 0, 4);
                    int remotePort = 0;
                    if (BitConverter.IsLittleEndian) {
                        remotePort = BitConverter.ToInt32(buffer.Reverse().ToArray(), 0);
                    } else {
                        remotePort = BitConverter.ToInt32(buffer, 0);
                    }

                    //确认是否是来自自己的数据包
                    if (iPAddresses.Where(item => item.IP.ToString() == endPoint.Address.ToString()).Count() != 0) continue;

                    //查找该主机是否已经存在于主机列表
                    var result = hostInfos
                        .Where(item => item.Host.Equals(endPoint.Address.ToString()))
                        .FirstOrDefault();
                    if (result == null) {
                        var hostAdd = new HostInfo(endPoint.Address.ToString(), "online") {
                            LastOnline = DateTime.UtcNow,
                            Port = remotePort
                        };
                        hostInfos.Add(hostAdd);
                        HostListChanged?.Invoke(null, new HostListChangedEventArgs(hostAdd, true));
                    } else {
                        result.Status = "online";
                        result.LastOnline = DateTime.UtcNow;
                    }
                } catch { break; }
            }
        }

        private static void Send(IPAddress ip, IPAddress mask) {

            byte[] buffer = new byte[8];
            //标识序列：4 bytes
            buffer[0] = 0x10;
            buffer[1] = 0x11;
            buffer[2] = 0x22;
            buffer[3] = 0x23;
            //发送本地开放端口号(大端序)
            if (BitConverter.IsLittleEndian) {
                Array.Copy(BitConverter.GetBytes(HttpPort).Reverse().ToArray(), 0, buffer, 4, 4);
            } else {
                Array.Copy(BitConverter.GetBytes(HttpPort), 0, buffer, 4, 4);
            }

            //计算广播地址
            byte[] ipByte = ip.GetAddressBytes();
            byte[] maskByte = mask.GetAddressBytes();
            byte[] broadcastAddressByte = new byte[4];
            for (int i = 0; i < 4; i++) {
                broadcastAddressByte[i] = (byte) (ipByte[i] | ~maskByte[i]);
            }
            IPAddress broadcastAddress = new IPAddress(broadcastAddressByte);

            udpClient.SendAsync(buffer, buffer.Length, new IPEndPoint(broadcastAddress, port));
        }

        public static event EventHandler<HostListChangedEventArgs> HostListChanged;
    }

    class HostListChangedEventArgs : EventArgs {

        public bool IsAdd { get; }
        public HostInfo HostInfo { get; }
        public HostListChangedEventArgs(HostInfo hostInfo, bool isAdd) {
            HostInfo = hostInfo;
            IsAdd = isAdd;
        }
    }

}
