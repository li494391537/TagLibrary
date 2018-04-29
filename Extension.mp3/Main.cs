using Lirui.TagLibrary.ExtensionCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lirui.TagLibray.Extension {
    [ExtensionInfo(new string[] { "mp3" }, Author = "lirui", Version = "1.0.0.0")]
    public class Mp3 : AbstractExtension {

        public Mp3(string filename) : base(filename) { }

        private List<KeyValuePair<string, string>> tags = new List<KeyValuePair<string, string>>();
        private int length;
        private int hasRead;

        /// <summary>
        /// 获取MP3格式文件元信息（ID3v1、ID3v2）
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public override KeyValuePair<string, string>[] GetTags() {
            using (FileStream fs = File.Open(Filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                BufferedStream bufferedStream = new BufferedStream(fs, 1024 * 16);
                byte[] buffer = new byte[3];
                int count = bufferedStream.Read(buffer, 0, 3);
                string str = Encoding.ASCII.GetString(buffer);
                buffer = new byte[2];
                count = bufferedStream.Read(buffer, 0, 2);
                if (str != "ID3" || buffer[0] != 3 || buffer[1] != 0) {
                    return null;
                }
                bufferedStream.Seek(1, SeekOrigin.Current);
                GetLength(bufferedStream);
                GetTags(bufferedStream);
                return tags.ToArray();
            }
        }
        private void GetLength(Stream stream) {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            length = buffer[0] << 21 |
                     buffer[1] << 14 |
                     buffer[2] << 07 |
                     buffer[3];
            hasRead = 0;
        }
        private void GetTags(Stream stream) {
            while (hasRead < length) {
                byte[] buffer = new byte[10];
                hasRead += stream.Read(buffer, 0, 10);
                string frameID = Encoding.ASCII.GetString(buffer, 0, 4);
                int frameBodySize = buffer[4] << 24 |
                                    buffer[5] << 16 |
                                    buffer[6] << 08 |
                                    buffer[7];
                if (frameBodySize < 1) break;  //帧大小最小为1byte
                buffer = new byte[frameBodySize];
                hasRead += stream.Read(buffer, 0, frameBodySize);
                string data = string.Empty;
                switch (frameID) {
                case "TALB":
                    data = GetTextData(buffer);
                    tags.Add(new KeyValuePair<string, string>("专辑", data));
                    break;
                //case "TIT1":
                //    data = GetTextData(buffer);
                //    tags.Add(new KeyValuePair<string, string>("TIT1", data));
                //    break;
                case "TIT2":
                    data = GetTextData(buffer);
                    tags.Add(new KeyValuePair<string, string>("歌名", data));
                    break;
                case "TIT3":
                    data = GetTextData(buffer);
                    tags.Add(new KeyValuePair<string, string>("子标题", data));
                    break;
                case "TPE1":
                    data = GetTextData(buffer);
                    data.Split('/')
                        .ToList()
                        .ForEach(item => tags.Add(new KeyValuePair<string, string>("歌手", item)));
                    break;
                case "TPE2":
                    data = GetTextData(buffer);
                    tags.Add(new KeyValuePair<string, string>("乐队\\伴奏", data));
                    break;
                //case "TPE3":
                //    data = GetTextData(buffer);
                //    tags.Add(new KeyValuePair<string, string>("TPE3", data));
                //    break;
                }
            }
        }

        private string GetTextData(byte[] buffer) {
            if (buffer[0] == 0) {
                //使用ISO-8859-1编码
                return Encoding.GetEncoding("ISO-8859-1").GetString(buffer, 1, buffer.Length - 1);
            } else if (buffer[1] == 0xFF && buffer[2] == 0xFE) {
                //使用小端UTF16
                return Encoding.Unicode.GetString(buffer, 3, buffer.Length - 3);
            } else if (buffer[1] == 0xFE && buffer[2] == 0xFF) {
                //使用大端UTF16
                return Encoding.BigEndianUnicode.GetString(buffer, 3, buffer.Length - 3);
            }
            throw new NotImplementedException();
        }
    }
}
