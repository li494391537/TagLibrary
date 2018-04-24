using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Loading.xaml 的交互逻辑
    /// </summary>
    public partial class Loading : Window {
        public event EventHandler ClickCancel;
        public double Progress {
            get => progressBar.Value;
            set {
                if (value >= 100) {
                    Dispatcher.Invoke(() => progressBar.Value = 100);
                } else if (value <= 0) {
                    Dispatcher.Invoke(() => progressBar.Value = 0);
                } else {
                    Dispatcher.Invoke(() => progressBar.Value = value);
                }
            }
        }

        public Loading() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            ClickCancel?.Invoke(this, new EventArgs());
        }
    }
}
