using Lirui.TagLibrary.Models;
using Lirui.TagLibrary.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Lirui.TagLibrary.Windows {
    /// <summary>
    /// SetTag.xaml 的交互逻辑
    /// </summary>
    public partial class SetTag : Window {
        private List<TagInfo> tags = new List<TagInfo>();
        public event EventHandler<AddingTagEventArgs> AddingTag;
        public bool IsOK { get; private set; } = false;
        public List<TagInfo> Tags {
            set {
                tags = value;
                if (tagTreeView != null) {
                    tagTreeView.Tags = tags;
                }
            }
        }

        private List<TagInfo> selectedTags = new List<TagInfo>();
        public List<TagInfo> SelectedTags {
            get => selectedTags;
            set {
                selectedTags = value;
                if (tagSelectedTreeView != null) {
                    tagSelectedTreeView.Tags = selectedTags;
                    tagSelectedTreeView.ExpandAll();
                }
            }
        }

        public FileInfo File { get; set; }

        public SetTag() {
            InitializeComponent();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog() {
                Multiselect = false
            };
            openFileDialog.ShowDialog();
            fileName.Text = openFileDialog.FileName;
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
            selectedTags = tagSelectedTreeView.Tags;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            tagSelectedTreeView.HasContextMenu = false;
            fileName.Text = File.Name;
        }

        private void TagTreeView_AddingTag(object sender, AddingTagEventArgs e) {
            AddingTag?.Invoke(sender, e);
        }

        public void AddTag(TagInfo tagInfo) {
            tagTreeView.AddTag(new TagInfo[] { tagInfo });
        }
    }
}
