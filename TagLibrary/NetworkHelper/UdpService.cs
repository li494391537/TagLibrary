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
        public static int Port => port;
        private static readonly int port = 4456;
        private static UdpClient udpClient = new UdpClient(port);
        private static System.Timers.Timer timer = new System.Timers.Timer(5000);
        private static IEnumerable<IPAddress> BroadcastAddress;
        private static IEnumerable<(IPAddress IP, IPAddress Mask)> iPAddresses;
        private static Task task;
        private static CancellationTokenSource cancellationTokenSource;

        private static BindingList<HostInfo> hostInfos = new BindingList<HostInfo>();


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
            SetTask();
        }


        private static void SetTask() {
            cancellationTokenSource = new CancellationTokenSource();
            task = new Task(Receive, cancellationTokenSource.Token);
        }

        public static void StartSendHeartBeat() {
            timer.Elapsed += SendHeartBeat;
            timer.Start();
        }

        public static void StopSendHeartBeat() {
            timer.Stop();
            timer.Elapsed -= SendHeartBeat;
        }

        public static void StartReceive() {
            if (task.Status == TaskStatus.Running) return;
            task.Start();
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
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                var data = udpClient.Receive(ref endPoint);
                if (data.Length != 4) continue;
                var remotePort = BitConverter.ToInt32(data, 0);

                //确认是否是来自自己的数据包
                if (iPAddresses.Where(item => item.IP.ToString() == endPoint.Address.ToString()).Count() != 0) continue;

                //查找该主机是否已经存在于主机列表
                var result = hostInfos
                    .Where(item => item.Host.Equals(endPoint.Address.ToString()))
                    .FirstOrDefault();
                if (result == null) {
                    hostInfos.Add(new HostInfo(endPoint.Address.ToString(), "online") { LastOnline = DateTime.UtcNow, Port = remotePort });
                } else {
                    result.Status = "online";
                    result.LastOnline = DateTime.UtcNow;
                }

            }
        }

        private static void Send(IPAddress ip, IPAddress mask) {

            byte[] buffer = BitConverter.GetBytes(port);

            byte[] ipByte = ip.GetAddressBytes();
            byte[] maskByte = mask.GetAddressBytes();

            byte[] broadcastAddressByte = new byte[4];
            for (int i = 0; i < 4; i++) {
                broadcastAddressByte[i] = (byte) (ipByte[i] | ~maskByte[i]);
            }

            IPAddress broadcastAddress = new IPAddress(broadcastAddressByte);

            udpClient.SendAsync(buffer, buffer.Length, new IPEndPoint(broadcastAddress, port));
        }


        //public static event EventHandler<ReceivedDataEventArgs> ReceivedData;
    }

    class ReceivedDataEventArgs : EventArgs {

        public IPEndPoint IPEndPoint { get; }
        public object Data { get; }
        public ReceivedDataEventArgs(IPEndPoint iPEndPoint, object data) {
            IPEndPoint = iPEndPoint;
            Data = data;
        }
    }

}
