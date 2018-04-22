using System.Windows;

namespace Lirui.TagLibrary.Windows {
    /// <summary>
    /// ToFolder.xaml 的交互逻辑
    /// </summary>
    public partial class CopyToFolder : Window {
        public bool IsOK { get; private set; } = false;
        public string SelectedFolder { get; private set; }
        public string SelectedGroup { get; private set; }
        public CopyToFolder() {
            InitializeComponent();
        }
        public CopyToFolder(string[] groups) : this() {
            this.groups.ItemsSource = groups;
            this.groups.SelectedIndex = 0;
        }

        private void Button_SelectFolder_Click(object sender, RoutedEventArgs e) {
            using (var selectFolder = new System.Windows.Forms.FolderBrowserDialog()) {
                if (selectFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    textBox.Text = selectFolder.SelectedPath;
                }
            }
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e) {
            if (textBox.Text == "") {
                MessageBox.Show("请选择一个文件夹!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SelectedFolder = textBox.Text;
            SelectedGroup = groups.Text;
            IsOK = true;
            Close();
        }
    }
}
