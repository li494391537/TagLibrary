using System.ComponentModel;

namespace Lirui.TagLibrary.NetworkHelper {
    class HostInfo : INotifyPropertyChanged {
        public string Host { get; }
        private string status;
        public string Status {
            get => status;
            set {
                status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
            }
        }
        public HostInfo(string host, string status = "offline") { Host = host; Status = status; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
