using Lirui.TagLibrary.Models;
using Lirui.TagLibrary.NetworkHelper;
using Lirui.TagLibrary.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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

        private Task<TagInfo[]> getTagInfos;
        private Task<FileInfo[]> getFileInfos;
        private Task<FileTagMapper[]> getMappers;

        public RemoteWindow() {
            InitializeComponent();
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e) {
            var selectedFiles = fileListView.SelectedItems.Cast<FileInfo>();
            foreach (var selectedFile in selectedFiles) {
                var filename = selectedFile.UUID + selectedFile.Format;
                filename = await HttpService.GetFile(IPAddress, Port, filename, selectedFile.Name);
                var tags = mappers
                   .Where(item => item.FileId == selectedFile.Id)
                   .Join(this.tags, x => x.TagId, y => y.Id, (x, y) => y)
                   .ToArray();
                Downloaded?.Invoke(this, new DownloadedEventArgs(filename, tags));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            getTagInfos = HttpService.GetTags(IPAddress, Port);
            getFileInfos = HttpService.GetFiles(IPAddress, Port);
            getMappers = HttpService.GetMappers(IPAddress, Port);


            var loadingWindow = new Loading() {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var cancel = new CancellationTokenSource();
            void taskCancel(object _sender, EventArgs _e) {
                cancel.Cancel();
                Close();
            }
            loadingWindow.ClickCancel += taskCancel;
            
            
            var task = Task.Factory.StartNew(() => {
                while (!getTagInfos.IsCompleted) ;
                tags = getTagInfos.Result.ToList();
                Dispatcher.Invoke(() => tagTreeView.Tags = tags);
                loadingWindow.Progress = 100.00/3;
                while (!getFileInfos.IsCompleted) ;
                files = getFileInfos.Result.ToList();
                Dispatcher.Invoke(() => fileListView.ItemsSource = new BindingList<FileInfo>(files.ToList()));
                loadingWindow.Progress = 200.00/3;
                while (!getMappers.IsCompleted) ;
                mappers = getMappers.Result.ToList();
                loadingWindow.Progress = 100.00;
                loadingWindow.ClickCancel -= taskCancel;
                loadingWindow.Dispatcher.Invoke(()=> loadingWindow.Close());
            }, cancel.Token);

            loadingWindow.ShowDialog();
            
        }

        public event EventHandler<DownloadedEventArgs> Downloaded;

        private void TagTreeView_TagCheckChanged(object sender, TagCheckChangedEventArgs e) {
            IEnumerable<FileInfo> result;
            if (tagTreeView.SelectedTag.Count == 0) {
                result = files.ToList();
            }else {
                result = tagTreeView.SelectedTag
                    .Join(mappers, x => x.Id, y => y.TagId, (x, y) => y)
                    .Join(files, x => x.FileId, y => y.Id, (x, y) => y)
                    .Distinct()
                    .ToList();
            }
            
            var needAdd = result.Except(fileListView.ItemsSource as BindingList<FileInfo>).ToArray();
            var needDel = (fileListView.ItemsSource as BindingList<FileInfo>).Except(result).ToArray();
            foreach (var del in needDel) {
                (fileListView.ItemsSource as BindingList<FileInfo>).Remove(del);
            }
            foreach (var add in needAdd) {
                (fileListView.ItemsSource as BindingList<FileInfo>).Add(add);
            }
        }
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
