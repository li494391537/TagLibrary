using Lirui.TagLibrary.Models;
using Lirui.TagLibrary.Properties;
using Lirui.TagLibrary.UserControls;
using Lirui.TagLibrary.Utils;
using Lirui.TagLibrary.ExtensionCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Lirui.TagLibrary.Windows {
    /// <summary>
    /// AddFile.xaml 的交互逻辑
    /// </summary>
    public partial class AddFile : Window {

        public event EventHandler<AddingTagEventArgs> AddingTag;

        private List<TagInfo> tags = new List<TagInfo>();
        public List<TagInfo> Tags {
            get => tags;
            set {
                tags = value;
                if (tagTreeView != null) {
                    tagTreeView.Tags = tags;
                }
            }
        }

        public bool IsOK { get; private set; } = false;
        public string[] FileNames { get; private set; }
        public List<TagInfo> SelectedTags { get; private set; }

        public AddFile() {
            InitializeComponent();
            tagTreeView.HasContextMenu = true;
            tagSelectedTreeView.HasContextMenu = false;
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog() {
                Multiselect = true
            };
            openFileDialog.ShowDialog();
            fileName.Text = "";
            openFileDialog.FileNames.ToList().ForEach(x => fileName.Text += "\"" + System.IO.Path.GetFileName(x) + "\" ");
            FileNames = openFileDialog.FileNames;
            //使用插件自动生成Tag
            if (Settings.Default.isUseExtension) {
                var tmpList = ExtensionUtil.ExtensionType
                        .Where(item => item.Key.ToUpper() == System.IO.Path.GetExtension(openFileDialog.FileName).TrimStart('.').ToUpper())
                        .Select(item => item.Value)
                        .SelectMany(item => {
                            try {
                                return (Activator.CreateInstance(item, openFileDialog.FileName) as IExtensionCommon).GetTags();
                            } catch {
                                return new List<KeyValuePair<string, string>>() as IEnumerable<KeyValuePair<string, string>>;
                            }
                        })
                        .Select(item => new { Key = System.IO.Path.GetExtension(openFileDialog.FileName).TrimStart('.').ToUpper() + "#" + item.Key, item.Value });
                foreach (var file in FileNames) {
                    tmpList = ExtensionUtil.ExtensionType
                        .Where(item => item.Key.ToUpper() == System.IO.Path.GetExtension(file).TrimStart('.').ToUpper())
                        .Select(item => item.Value)
                        .SelectMany(item => {
                            try {
                                return (Activator.CreateInstance(item, file) as IExtensionCommon).GetTags();
                            } catch {
                                return new List<KeyValuePair<string, string>>() as IEnumerable<KeyValuePair<string, string>>;
                            }
                        })
                        .Select(item => new { Key = System.IO.Path.GetExtension(openFileDialog.FileName).TrimStart('.').ToUpper() + "#" + item.Key, item.Value })
                        .Join(tmpList, x => (x.Key, x.Value), y => (y.Key, y.Value), (x, y) => x);
                    //.Select(item => new TagInfo() { Group = System.IO.Path.GetExtension(file).TrimStart('.').ToUpper() + "#" + item.Key, Name = item.Value })
                    //.ToList();
                    //tagSelectedTreeView.Tags = tags;
                }
                tagSelectedTreeView.Tags = 
                    tmpList
                    .Select(item => new TagInfo() { Group = item.Key, Name = item.Value })
                    .ToList();
            }
        }

        private void SelectTag_Click(object sender, RoutedEventArgs e) {
            var selectTag = tagTreeView.SelectedTag;
            var selectedTag = tagSelectedTreeView.Tags;
            tagSelectedTreeView.AddTag(selectTag.Except(selectedTag).ToArray());
            tagSelectedTreeView.ExpandAll();
        }

        private void CancelSelectTag_Click(object sender, RoutedEventArgs e) {
            var tags = tagSelectedTreeView.SelectedTag;
            tagTreeView.RemoveSelectedTag(tags.ToArray());
            tagSelectedTreeView.RemoveTag(tags.ToArray(), true);
        }

        private void AddTag_Click(object sender, RoutedEventArgs e) {
            var addTagWindow = new AddTag() {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            addTagWindow.ShowDialog();
            if (addTagWindow.IsOK) {
                var tagInfo = new TagInfo() {
                    Name = addTagWindow.TagName,
                    Group = addTagWindow.TagGroup
                };
                AddingTag?.Invoke(this, new AddingTagEventArgs(tagInfo));
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e) {
            IsOK = true;
            SelectedTags = tagSelectedTreeView.Tags;
            if (!FileNames.All(x => System.IO.File.Exists(x))) {
                MessageBox.Show("文件不存在！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void TagTreeView_AddingTag(object sender, AddingTagEventArgs e) {
            AddingTag?.Invoke(sender, e);
        }


        public void AddTag(TagInfo tagInfo) {
            tagTreeView.AddTag(new TagInfo[] { tagInfo });
        }
    }
}
