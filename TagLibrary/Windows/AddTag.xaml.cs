using System.Windows;

namespace Lirui.TagLibrary.Windows {
    /// <summary>
    /// AddTag.xaml 的交互逻辑
    /// </summary>
    public partial class AddTag : Window {

        public AddTag() {
            InitializeComponent();
        }

        public string TagGroup { get; private set; } = string.Empty;
        public string TagName { get; private set; } = string.Empty;
        public bool IsOK { get; private set; } = false;

        private void ButtonOK_Click(object sender, RoutedEventArgs e) {
            TagGroup = tagGroup.Text;
            TagName = tagName.Text;
            IsOK = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
