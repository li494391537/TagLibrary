using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Lirui.TagLibrary.Models;
using Lirui.TagLibrary.Windows;

namespace Lirui.TagLibrary.UserControls {
    /// <summary>
    /// TagTreeView.xaml 的交互逻辑
    /// </summary>
    public partial class TagTreeView : UserControl {

        #region Event
        public event EventHandler<TagCheckChangedEventArgs> TagCheckChanged;
        public event EventHandler<AddingTagEventArgs> AddingTag;
        #endregion

        #region Property

        public bool HasContextMenu { get; set; } = true;

        public List<TagInfo> Tags {
            get {
                return (tagTree.ItemsSource as BindingList<TreeViewItem>)
                       .SelectMany(x => x.ItemsSource as BindingList<CheckBox>)
                       .Select(x => x.DataContext as TagInfo)
                       .ToList();
            }
            set {
                Dispatcher.Invoke(() => {
                    var tags = value;
                    var groups = new BindingList<TreeViewItem>();
                    tagTree.ItemsSource = groups;
                    foreach (var item in tags.GroupBy(item => item.Group).Select(item => item.First())) {
                        var group = new TreeViewItem() {
                            DataContext = item,
                            ItemsSource = new BindingList<CheckBox>()
                        };
                        group.SetBinding(TreeViewItem.HeaderProperty, new Binding("Group"));
                        groups.Add(group);
                    }
                    foreach (var item in tags) {
                        var tag = new CheckBox() { DataContext = item };
                        tag.SetBinding(ContentProperty, new Binding("Name"));
                        tag.Checked += Tag_CheckChanged;
                        tag.Unchecked += Tag_CheckChanged;
                        (groups.Where(group => (group.DataContext as TagInfo).Group == item.Group).First().ItemsSource as BindingList<CheckBox>)
                            .Add(tag);
                    }
                    tagTree.ItemsSource = groups;
                });
            }
        }

        public List<TagInfo> SelectedTag {
            get {
                return
                    (tagTree.ItemsSource as BindingList<TreeViewItem>)
                    .SelectMany(x => x.ItemsSource as BindingList<CheckBox>)
                    .Where(x => x.IsChecked ?? false)
                    .Select(x => x.DataContext as TagInfo)
                    .ToList();
            }
            set {
                (tagTree.ItemsSource as BindingList<TreeViewItem>)
                    .SelectMany(x => x.ItemsSource as BindingList<CheckBox>)
                    .ToList()
                    .ForEach(x => x.IsChecked = false);
                (tagTree.ItemsSource as BindingList<TreeViewItem>)
                    .SelectMany(x => x.ItemsSource as BindingList<CheckBox>)
                    .Join(value, x => x.DataContext as TagInfo, y => y, (x, y) => x)
                    .ToList()
                    .ForEach(x => x.IsChecked = true);
            }
        }

        #endregion

        /// <summary>
        /// 构造方法
        /// </summary>
        public TagTreeView() {
            InitializeComponent();
            tagTree.ItemsSource = new BindingList<TreeViewItem>();
        }

        #region 右键菜单的事件处理方法

        /// <summary>
        /// 右键->添加Tag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenuAddTag_Click(object sender, RoutedEventArgs e) {
            var addTagWindow = new AddTag() {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            addTagWindow.ShowDialog();
            if (addTagWindow.IsOK) {
                AddingTag?.Invoke(this, new AddingTagEventArgs(new TagInfo {
                    Group = addTagWindow.TagGroup,
                    Name = addTagWindow.TagName
                }));
            }
        }

        /// <summary>
        /// 右键->全部折叠
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenuCollapseAll_Click(object sender, RoutedEventArgs e) {
            CollapseAll();
        }

        /// <summary>
        /// 右键->全部展开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenuExpandAll_Click(object sender, RoutedEventArgs e) {
            ExpandAll();
        }

        /// <summary>
        /// 右键->全部选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenuCheckAll_Click(object sender, RoutedEventArgs e) {
            (tagTree.ItemsSource as BindingList<TreeViewItem>)
                .SelectMany(x => x.ItemsSource as BindingList<CheckBox>)
                .ToList()
                .ForEach(x => x.IsChecked = true);
            ExpandAll();
        }

        /// <summary>
        /// 右键->全部取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemUncheckAll_Click(object sender, RoutedEventArgs e) =>
            (tagTree.ItemsSource as BindingList<TreeViewItem>)
                .SelectMany(x => x.ItemsSource as BindingList<CheckBox>)
                .ToList()
                .ForEach(x => x.IsChecked = false);

        /// <summary>
        /// 右键->反向选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenuInverse_Click(object sender, RoutedEventArgs e) =>
            (tagTree.ItemsSource as BindingList<TreeViewItem>)
                .SelectMany(x => x.ItemsSource as BindingList<CheckBox>)
                .ToList()
                .ForEach(x => x.IsChecked = !x.IsChecked);


        #endregion

        #region 其他事件的处理方法

        /// <summary>
        /// 选择项变化时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tag_CheckChanged(object sender, RoutedEventArgs e) {
            var selectTag = (tagTree.ItemsSource as BindingList<TreeViewItem>)
                            .SelectMany(x => x.ItemsSource as BindingList<CheckBox>)
                            .Where(x => x.IsChecked == true)
                            .Select(x => x.DataContext as TagInfo);
            TagCheckChanged?.Invoke(this, new TagCheckChangedEventArgs(selectTag.ToArray()));
        }

        /// <summary>
        /// 右键菜单打开时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagTree_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
            if (!HasContextMenu) {
                tagTreeContextMenu.IsEnabled = false;
                tagTreeContextMenu.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region 折叠、展开

        public void CollapseAll() {
            (tagTree.ItemsSource as BindingList<TreeViewItem>)
                .ToList()
                .ForEach(x => x.IsExpanded = false);
        }

        public void ExpandAll() {
            (tagTree.ItemsSource as BindingList<TreeViewItem>)
                .ToList()
                .ForEach(x => x.IsExpanded = true);

        }

        #endregion

        #region 各种Public方法
        /// <summary>
        /// 向TreeView中添加Tags，PS：不会更改Tags，Tags只用来初始化TreeView，不会监测其变化
        /// </summary>
        /// <param name="tagInfos"></param>
        public void AddTag(TagInfo[] tagInfos) {
            //唯一性约束检查
            //foreach (var tagInfo in tagInfos.Join(tags, x => new { x.Name, x.Group }, y => new { y.Name, y.Group }, (x, y) => x)) {
            //    tagInfos.Remove(tagInfo);
            //}

            foreach (var tagInfo in tagInfos) {
                //tags.Add(tagInfo);
                var tag = new CheckBox() { DataContext = tagInfo };
                tag.SetBinding(ContentProperty, new Binding("Name"));
                tag.Checked += Tag_CheckChanged;
                tag.Unchecked += Tag_CheckChanged;
                var groups = tagTree.ItemsSource as BindingList<TreeViewItem>;
                if (groups.Where(item => (item.DataContext as TagInfo).Group == tagInfo.Group).Count() == 0) {
                    var group = new TreeViewItem() {
                        DataContext = tagInfo,
                        ItemsSource = new BindingList<CheckBox>() { tag }
                    };
                    group.SetBinding(HeaderedItemsControl.HeaderProperty, new Binding("Group"));
                    groups.Add(group);
                } else {
                    (groups.Where(group => (group.DataContext as TagInfo).Group == tagInfo.Group).First().ItemsSource as BindingList<CheckBox>)
                        .Add(tag);
                }
            }
        }

        /// <summary>
        /// 向TreeView中删除Tags，PS：不会更改Tags，Tags只用来初始化TreeView，不会监测其变化
        /// </summary>
        /// <param name="tagIds"></param>
        /// <param name="isRemoveGroup"></param>
        public void RemoveTag(TagInfo[] tagInfos, bool isRemoveGroup = false) {
            (tagTree.ItemsSource as BindingList<TreeViewItem>)
                .SelectMany(x => x.ItemsSource as BindingList<CheckBox>, (x, y) => new { group = x, tag = y })
                .Join(tagInfos, x => x.tag.DataContext as TagInfo, y => y, (x, y) => x)
                .ToList()
                .ForEach(x => {
                    (x.group.ItemsSource as BindingList<CheckBox>).Remove(x.tag);
                    if (!x.group.HasItems && isRemoveGroup) {
                        (tagTree.ItemsSource as BindingList<TreeViewItem>).Remove(x.group);
                    }
                });

            //foreach (var tag in tagIds.Join(tags, x => x, y => y.Id, (x, y) => y)) {
            //    ///tags.Remove(tag);
            //    var groupBox = (tagTree.ItemsSource as BindingList<TreeViewItem>).Where(item => (item.DataContext as TagInfo).Group == tag.Group).First();
            //    var tagBox = (groupBox.ItemsSource as BindingList<CheckBox>).Where(item => (item.DataContext as TagInfo).Name == tag.Name).First();
            //    (groupBox.ItemsSource as BindingList<CheckBox>).Remove(tagBox);
            //}
            //if (isRemoveGroup) {
            //    foreach (var item in (tagTree.ItemsSource as BindingList<TreeViewItem>).Where(item => (item.ItemsSource as BindingList<CheckBox>).Count == 0).ToList()) {
            //        (tagTree.ItemsSource as BindingList<TreeViewItem>).Remove(item);
            //    }
            //}
        }

        /// <summary>
        /// 在TreeView中更改Tags，PS：不会更改Tags，Tags只用来初始化TreeView，不会监测其变化
        /// </summary>
        /// <param name="tagInfo"></param>
        /// <returns></returns>
        public void UpdateTag(TagInfo tagInfo) {
            var tag = (tagTree.ItemsSource as BindingList<TreeView>)
                      .SelectMany(x => x.ItemsSource as BindingList<CheckBox>)
                      .Select(x => x.DataContext as TagInfo)
                      .Where(x => x.Id == tagInfo.Id)
                      .First();
            tagInfo.Name = tagInfo.Name ?? tag.Name;
            tagInfo.Group = tagInfo.Group ?? tag.Group;
            RemoveTag(new TagInfo[] { tagInfo });
            AddTag(new TagInfo[] { tagInfo });
        }

        //public void AddSelectedTag(List<int> tagIds) {
        //    List<CheckBox> allTags = new List<CheckBox>();
        //    foreach (var group in tagTree.ItemsSource as BindingList<TreeViewItem>) {
        //        foreach (var tag in group.ItemsSource as BindingList<CheckBox>) {
        //            allTags.Add(tag);
        //        }
        //    }
        //    foreach (var tag in allTags.Join(tagIds, x => (x.DataContext as TagInfo).Id, y => y, (x, y) => x)) {
        //        tag.IsChecked = true;
        //    }
        //}

        public void AddSelectedTag(TagInfo[] tagInfos) {
            (tagTree.ItemsSource as BindingList<TreeViewItem>)
                .SelectMany(x => x.ItemsSource as BindingList<CheckBox>)
                .Join(tagInfos, x => x.DataContext as TagInfo, y => y, (x, y) => x)
                .ToList()
                .ForEach(x => x.IsChecked = true);
        }

        public void RemoveSelectedTag(TagInfo[] tagInfos) {
            (tagTree.ItemsSource as BindingList<TreeViewItem>)
                .SelectMany(x => x.ItemsSource as BindingList<CheckBox>)
                .Join(tagInfos, x => x.DataContext as TagInfo, y => y, (x, y) => x)
                .ToList()
                .ForEach(x => x.IsChecked = false);
        }
        #endregion

    }

    public class TagCheckChangedEventArgs : EventArgs {
        public TagInfo[] Tags { get; }
        public TagCheckChangedEventArgs(TagInfo[] tags) {
            Tags = tags;
        }
    }

    public class AddingTagEventArgs: EventArgs {
        public TagInfo Tag { get; }
        public AddingTagEventArgs(TagInfo tag) {
            Tag = tag;
        }
    }
}
