using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lirui.TagLibrary.NetworkHelper {
    static class HttpService {
        private static HttpListener httpListener = new HttpListener();
        private static int Port;
        private static Task task;
        private static CancellationTokenSource cancellationTokenSource;
        static HttpService() {
            var random = new Random();
            Port = random.Next() % 10000 + 40000;
            while (IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Where(item => item.Port == Port).Count() != 0) {
                Port = random.Next() % 10000 + 40000;
            }
            httpListener.Prefixes.Add("http://+:" + Port + "/");
            task = new Task(Teststt, cancellationTokenSource.Token);
        }

        public static void StartHttpService() {
            try {
                httpListener.Start();
                task.Start();
            } catch { }
        }

        public static void StopHttpService() {
            try {
                cancellationTokenSource.Cancel();
                httpListener.Stop();
            } catch { }
        }

        private static void Teststt() {
            while (true) {
                var context = httpListener.GetContext();
            }
        }

        public static async Task Send(IPAddress ip, int port) {
            
            
        }

        public static async Task RequestFileList(IPAddress ip, int port) {
            var httpClient = new HttpClient();
            var res = await httpClient.SendAsync(new HttpRequestMessage());
            if (res.IsSuccessStatusCode) return;
            var content = await res.Content.ReadAsStreamAsync();
        }
    }
}