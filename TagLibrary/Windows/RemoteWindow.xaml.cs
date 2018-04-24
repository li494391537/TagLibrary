using Lirui.TagLibrary.Models;
using Lirui.TagLibrary.NetworkHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lirui.TagLibrary.Windows {
    /// <summary>
    /// RemoteWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RemoteWindow : Window {

        public IPAddress IPAddress { get; set; }
        public int Port { get; set; }

        private List<TagInfo> tags;
        private List<FileInfo> files;
        private List<FileTagMapper> mappers;

        public RemoteWindow() {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            var selectedFiles = fileListView.SelectedItems.Cast<FileInfo>();
            foreach (var selectedFile in selectedFiles) {
                var filename = selectedFile.UUID + selectedFile.Format;
                filename = HttpService.GetFile(IPAddress, Port, filename).Result;
                var tags = mappers
                   .Where(item => item.FileId == selectedFile.Id)
                   .Join(this.tags, x => x.TagId, y => y.Id, (x, y) => y)
                   .ToArray();
                Downloaded?.Invoke(this, new DownloadedEventArgs(filename, tags));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            tags = HttpService.GetTags(IPAddress, Port).Result.ToList();
            files = HttpService.GetFiles(IPAddress, Port).Result.ToList();
            mappers = HttpService.GetMappers(IPAddress, Port).Result.ToList();
            tagTreeView.Tags = tags;
            fileListView.ItemsSource = files;
        }

        public event EventHandler<DownloadedEventArgs> Downloaded;
    }

    public class DownloadedEventArgs : EventArgs {
        public TagInfo[] TagInfos { get; }
        public string FileName { get; }

        public DownloadedEventArgs(string filename, TagInfo[] tagInfos) {
            FileName = filename;
            TagInfos = tagInfos;
        }

    }
}
