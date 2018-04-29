using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lirui.TagLibrary.NetworkHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;
using System.IO;

namespace Lirui.TagLibrary.NetworkHelper.Tests {
    [TestClass()]
    public class HttpServiceTests {

        [TestMethod()]
        public void GetTagsTest() {
            try {
                HttpService.TagInfos = new List<Models.TagInfo> {
                    new Models.TagInfo() {
                        Id = 1,
                        Group = "Group1",
                        Name = "Name1"
                    },
                    new Models.TagInfo() {
                        Id = 2,
                        Group = "Group2",
                        Name = "Name2"
                    }
                };
                HttpService.StartHttpService();
                var result = HttpService.GetTags(IPAddress.Parse("192.168.31.11"), 48972).Result;
                HttpService.StopHttpService();
                if (HttpService.TagInfos.Count != result.Length) Assert.Fail();
                for (int i = 0; i < HttpService.TagInfos.Count; i++) {
                    if (HttpService.TagInfos[i].Id != result[i].Id 
                        || HttpService.TagInfos[i].Group != result[i].Group 
                        || HttpService.TagInfos[i].Name != result[i].Name) {
                        Assert.Fail();
                    }
                }
            } catch { Assert.Fail(); }

        }

        [TestMethod()]
        public void GetFileTest() {
            try {
                HttpService.StartHttpService();
                var filename = HttpService.GetFile(IPAddress.Parse("127.0.0.1"), HttpService.Port, "61a0883194684064ae8cabc63ea5b805.jpg", "test.jpg").Result;
                HttpService.StopHttpService();

                var md5 = new MD5CryptoServiceProvider();

                var file1 = File.OpenRead(Environment.CurrentDirectory + @"\library\" + "61a0883194684064ae8cabc63ea5b805.jpg");
                var md5_1 = md5.ComputeHash(file1);
                var file2 = File.OpenRead(filename);
                var md5_2 = md5.ComputeHash(file2);
                for (int i = 0; i < md5_1.Length; i++) {
                    if (md5_1[i] != md5_2[i]) Assert.Fail();
                }

            } catch { Assert.Fail(); }
        }

        [TestMethod()]
        public void GetFilesTest() {
            try {
                HttpService.FileInfos = new List<Models.FileInfo> {
                    new Models.FileInfo() {
                        Id = 1,
                        Name = "Name1"
                    },
                    new Models.FileInfo() {
                        Id = 2,
                        Name = "Name2"
                    }
                };
                HttpService.StartHttpService();
                var result = HttpService.GetFiles(IPAddress.Parse("127.0.0.1"), HttpService.Port).Result;
                HttpService.StopHttpService();


                if (HttpService.FileInfos.Count != result.Length) Assert.Fail();
                for (int i = 0; i < HttpService.FileInfos.Count; i++) {
                    if (HttpService.FileInfos[i].Id != result[i].Id ||
                        HttpService.FileInfos[i].Name != result[i].Name) {
                        Assert.Fail();
                    }
                }
            } catch { Assert.Fail(); }
        }
    }
}