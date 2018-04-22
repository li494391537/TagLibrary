using Lirui.TagLibrary.Properties;
using System.Linq;
using System.Windows;

namespace Lirui.TagLibrary.Windows {
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Window {
        public Setting() {
            InitializeComponent(); setting1.IsSelected = true;
            setting1.Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e) {
            Settings.Default.isUseExtension = isUseExtension.IsChecked ?? false;
            Settings.Default.Save();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Apply_Click(object sender, RoutedEventArgs e) {
            Settings.Default.isUseExtension = isUseExtension.IsChecked ?? false;
            Settings.Default.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            isUseExtension.IsChecked = Settings.Default.isUseExtension; 
            var extensionArray = Lirui.TagLibrary.Utils.ExtensionUtil.Assemblies
                .Select(item => {
                    string name = string.Empty;
                    name = item.Key.GetName().Name + ".dll";
                    string format = string.Empty;
                    item.Value.ForEach(str => format += str + ',');
                    format = format.TrimEnd(',');
                    return new { Name = name, Format = format };
                })
                .ToArray();
            extensionListView.ItemsSource = extensionArray;
        }
    }
}
