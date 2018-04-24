﻿using Lirui.TagLibrary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Lirui.TagLibrary.NetworkHelper {
    public static class HttpService {

        private static HttpListener httpListener = new HttpListener();
        private static int port;
        public static int Port { get => port; }
        private static Task task;
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private static Regex file = new Regex(@"^\/File\/(?<filename>[0-9a-fA-F]{32,32})\.(?<format>\w{1,7})$");
        private static Regex fileInfos = new Regex(@"^\/FileInfos$");
        private static Regex tagInfos = new Regex(@"^\/TagInfos$");

        public static TagInfo[] TagInfos { get; set; }
        public static Models.FileInfo[] FileInfos { get; set; }


        static HttpService() {
            var random = new Random();
            port = random.Next() % 10000 + 40000;
            while (IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Where(item => item.Port == Port).Count() != 0) {
                port = random.Next() % 10000 + 40000;
            }
            httpListener.Prefixes.Add("http://+:" + Port + "/");
            task = new Task(Server, cancellationTokenSource.Token);
        }

        public static bool StartHttpService() {
            try {
                httpListener.Start();
                task.Start();
                return true;
            } catch { return false; }
        }

        public static bool StopHttpService() {
            try {
                cancellationTokenSource.Cancel();
                httpListener.Stop();
                return true;
            } catch { return false; }
        }

        private static void Server() {
            while (true) {
                HttpListenerContext context;
                try {
                    context = httpListener.GetContext();
                } catch { break; }

                var req = context.Request;
                var res = context.Response;
                try {
                    if (file.IsMatch(req.RawUrl)) {

                        var result = file.Match(req.RawUrl);
                        var filename = result.Groups["filename"].Value;
                        var format = result.Groups["format"].Value;
                        var fileBytes = File.ReadAllBytes(Environment.CurrentDirectory + @"\library\" + filename + '.' + format);
                        res.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                        res.StatusCode = 200;

                    } else if (tagInfos.IsMatch(req.RawUrl)) {
                        
                        var json = JsonConvert.SerializeObject(TagInfos);
                        var jsonBytes = Encoding.UTF8.GetBytes(json);
                        res.OutputStream.Write(jsonBytes, 0, jsonBytes.Length);
                        res.StatusCode = 200;

                    } else if (fileInfos.IsMatch(req.RawUrl)) {
                        
                        var json = JsonConvert.SerializeObject(FileInfos);
                        var jsonBytes = Encoding.UTF8.GetBytes(json);
                        res.OutputStream.Write(jsonBytes, 0, jsonBytes.Length);
                        res.StatusCode = 200;

                    } else {
                        res.StatusCode = 404;
                    }

                } catch {
                    res.StatusCode = 500;
                } finally {
                    res.Close();
                }

            }
        }

        public static async Task<Models.FileInfo[]> GetFiles(IPAddress ip, int port) {
            try {
                var httpClient = new HttpClient();
                var req = new HttpRequestMessage {
                    RequestUri = new Uri("http://" + ip.ToString() + ":" + port + "/FileInfos")
                };
                var res = await httpClient.SendAsync(req);
                if (!res.IsSuccessStatusCode) return null;
                var jsonBytes = await res.Content.ReadAsByteArrayAsync();
                var json = Encoding.UTF8.GetString(jsonBytes);
                var fileInfos = JsonConvert.DeserializeObject<Models.FileInfo[]>(json);
                return fileInfos;
            } catch { return null; }
        }

        public static async Task<string> GetFile(IPAddress ip, int port, string filename) {
            try {
                var httpClient = new HttpClient();
                var req = new HttpRequestMessage {
                    RequestUri = new Uri("http://" + ip.ToString() + ":" + port + "/File/" + filename)
                };
                var res = await httpClient.SendAsync(req);
                if (!res.IsSuccessStatusCode) return null;
                var contentBytes = await res.Content.ReadAsByteArrayAsync();
                var tempPath = Path.GetTempPath();
                var tempDirectory = tempPath + Path.GetRandomFileName();
                Directory.CreateDirectory(tempDirectory);
                File.WriteAllBytes(tempDirectory + '\\' + filename, contentBytes);
                return tempDirectory + '\\' + filename;
            } catch { return null; }

        }

        public static async Task<TagInfo[]> GetTags(IPAddress ip, int port) {
            try {
                var httpClient = new HttpClient();
                var req = new HttpRequestMessage {
                    RequestUri = new Uri("http://" + ip.ToString() + ":" + port + "/TagInfos")
                };
                var res = await httpClient.SendAsync(req);
                if (!res.IsSuccessStatusCode) return null;
                var jsonBytes = await res.Content.ReadAsByteArrayAsync();
                var json = Encoding.UTF8.GetString(jsonBytes);
                var tagInfos = JsonConvert.DeserializeObject<TagInfo[]>(json);
                return tagInfos;
            } catch { return null; }
        }
    }
}