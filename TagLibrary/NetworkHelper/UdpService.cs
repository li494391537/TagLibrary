using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace Lirui.TagLibrary.NetworkHelper {
    class UdpService {
        private static readonly int port = 4456;
        public static int Port => port;

        private static System.Timers.Timer timer = new System.Timers.Timer(5);

        private static UdpClient udpClient = new UdpClient(port, AddressFamily.InterNetwork);


        public static void SetBroadcast() {
            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            var t = NetworkInterface.GetAllNetworkInterfaces()
                .Where(item => item.OperationalStatus == OperationalStatus.Up && item.SupportsMulticast == true);
            var t1 = t.SelectMany(item => item.GetIPProperties().UnicastAddresses)
                .Where(item => item.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(item => new { IP = item.Address, Mask = item.IPv4Mask });
        }

        public static void StartSendHeartBeat() {
            timer.Elapsed += SendHeartBeat;
            timer.Start();
        }

        public static void StopSendHeartBeat() {
            timer.Stop();
            timer.Elapsed -= SendHeartBeat;
        }

        private static void SendHeartBeat(object sender, System.Timers.ElapsedEventArgs e) {
            var des = new IPEndPoint(IPAddress.Parse("192.168.31.255"), port);
            udpClient.Send(Encoding.ASCII.GetBytes("hbuckwpzg"), 9, des);
        }
    }
}
