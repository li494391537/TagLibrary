using System;
using System.ComponentModel;

namespace Lirui.TagLibrary.NetworkHelper {
    class HostInfo : INotifyPropertyChanged {
        private string status;
        public string Status {
            get => status;
            set {
                status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
            }
        }
        public string Host { get; }
        public DateTime LastOnline { get; set; }
        public int Port { get; set; }

        public HostInfo(string host, string status = "offline") { Host = host; Status = status; LastOnline = DateTime.MinValue; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
