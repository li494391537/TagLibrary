using Lirui.TagLibrary.Models;
using Lirui.TagLibrary.NetworkHelper;
using Lirui.TagLibrary.UserControls;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Lirui.TagLibrary.Windows {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IDisposable {

        private string version;
        private int versionMajor = 0;
        private int versionMinor = 1;
        private int versionBuild = 0;

        SqlSugarClient db;

        List<FileInfo> files;
        List<TagInfo> tags;
        List<FileTagMapper> mappers;
        //BindingList<HostInfo> hosts = new BindingList<HostInfo>();

        /// <summary>
        /// 构造方法
        /// </summary>
        public MainWindow() {

            InitializeComponent();
            //初始化存储目录
            System.IO.Directory.CreateDirectory("library");

            #region TEST
            //System.IO.File.Delete("data.db");
            #endregion

            #region 初始化数据库

            #region sqlite文件检查 (暂时未实现)
            //try {
            //    bool isTableExists;
            //    using (var sqlite = new SQLiteConnection("Data Source=data.db;Version=3;")) {
            //        sqlite.Open();
            //        var tableExists = new SQLiteCommand("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='TagLibrary';", sqlite);
            //        var result = tableExists.ExecuteReader();
            //        result.Read();
            //        if (result.GetInt32(0) == 0) {
            //            isTableExists = false;
            //        } else {
            //            isTableExists = true;
            //        }
            //        sqlite.Close();
            //    }

            //    if (!isTableExists) {
            //        GC.Collect();
            //        GC.WaitForPendingFinalizers();
            //        File.Delete("data.db");
            //        using (var sqlite = new SQLiteConnection("Data Source=data.db;Version=3;")) {
            //            sqlite.Open();
            //            var createTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS TagLibrary (Major INT, Minor INT, Build INT);", sqlite);
            //            createTable.ExecuteNonQuery();
            //            sqlite.Close();
            //        }
            //    }
            //} catch {
            //    GC.Collect();
            //    GC.WaitForPendingFinalizers();
            //    File.Delete("data.db");
            //    using (var sqlite = new SQLiteConnection("Data Source=data.db;Version=3;")) {
            //        sqlite.Open();
            //        var createTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS TagLibrary (Major INT, Minor INT, Build INT);", sqlite);
            //        createTable.ExecuteNonQuery();
            //        sqlite.Close();
            //    }
            //}
            #endregion

            //初始化SqlSugar
            db = new SqlSugarClient(new ConnectionConfig() {
                DbType = DbType.Sqlite,
                ConnectionString = "Data Source=data.db;Version=3;",
                InitKeyType = InitKeyType.Attribute
            });
            //检查版本并进行版本迁移工作
            CheckVersion();
            CodeFirst();
            #endregion

            #region TEST
            //TestData();
            #endregion

            #region 初始化字段
            version = string.Format("{0}.{1}.{2:000000}", versionMajor, versionMinor, versionBuild);
            #endregion

            #region 从数据库中读取数据



            //初始化文件列表
            files = db.Queryable<FileInfo>().ToList();
            fileList.ItemsSource = new BindingList<FileInfo>();
            foreach (var item in files) {
                (fileList.ItemsSource as BindingList<FileInfo>).Add(item);
            }

            //初始化文件格式列表
            //var fileFormats = new BindingList<TreeViewItem>();
            //fileList.GroupBy(fileInfo => fileInfo.Format)
            //    .Select(fileInfo => fileInfo.Key)
            //    .ToList()
            //    .ForEach(format => fileFormats.Add(new TreeViewItem() {
            //        Header = format
            //    }));
            //tagTree.ItemsSource = fileFormats;

            //初始化Tag列表
            tags = db.Queryable<TagInfo>().ToList();
            //var groups = new BindingList<TreeViewItem>();
            //foreach (var item in tags.GroupBy(item => item.Group).Select(item => item.First())) {
            //    var group = new TreeViewItem() {
            //        DataContext = item,
            //        ItemsSource = new BindingList<CheckBox>()
            //    };
            //    group.SetBinding(TreeViewItem.HeaderProperty, new Binding("Group"));
            //    groups.Add(group);
            //}
            //foreach (var item in tags) {
            //    var tag = new CheckBox() { DataContext = item };
            //    tag.SetBinding(ContentProperty, new Binding("Name"));
            //    tag.Checked += Tag_Checked;
            //    tag.Unchecked += Tag_Checked;
            //    (groups.Where(group => (group.DataContext as TagInfo).Group == item.Group).First().ItemsSource as BindingList<CheckBox>)
            //        .Add(tag);
            //}
            tagTree.Tags = tags;
            //foreach (var item in tags) {
            //    var tag = new CheckBox() { DataContext = item };
            //    tag.SetBinding(ContentProperty, new Binding("Name"));
            //    tag.Checked += Tag_Checked;
            //    tag.Unchecked += Tag_Checked;
            //    if ((tagTree.ItemsSource as BindingList<TreeViewItem>).Where(group => (group.DataContext as TagInfo).Group == item.Group).Count() == 0) {
            //        var group = new TreeViewItem() {
            //            DataContext = item,
            //            ItemsSource = new BindingList<CheckBox>() { tag }
            //        };
            //        group.SetBinding(HeaderedItemsControl.HeaderProperty, new Binding("Group"));
            //        (tagTree.ItemsSource as BindingList<TreeViewItem>).Add(group);
            //    } else {
            //        ((tagTree.ItemsSource as BindingList<TreeViewItem>).Where(group => (group.DataContext as TagInfo).Group == item.Group).First().ItemsSource as BindingList<CheckBox>)
            //            .Add(tag);
            //    }
            //}

            //初始化映射列表
            mappers = db.Queryable<FileTagMapper>().ToList();

            //hostSelect.ItemsSource = hosts;
            //hosts.Add(new HostInfo("localhost"));
            hostList.ItemsSource = new BindingList<HostInfo>(UdpService.HostInfos);
            UdpService.HostListChanged += (o, e) => {
                hostList.Dispatcher.Invoke(() => {
                    if (e.IsAdd) {
                        (hostList.ItemsSource as BindingList<HostInfo>).Add(e.HostInfo);
                    }else {
                        (hostList.ItemsSource as BindingList<HostInfo>).Remove(e.HostInfo);
                    }
                });

            };
            //hostSelect.SelectedIndex = 0;

            #endregion

            #region 关联事件
            #endregion

        }

        #region 菜单栏事件处理方法

        private void Test_Click(object sender, RoutedEventArgs e) {
            //new TextRange(test1.Document.ContentStart, test1.Document.ContentEnd).Text = "";
            //db.Deleteable<TagInfo>(item => 1 == 1).ExecuteCommand();
            //db.Deleteable<FileTagMapper>(item => 1 == 1).ExecuteCommand();
            UdpService.StartSendHeartBeat();
            UdpService.StartReceive();
        }

        /// <summary>
        /// 菜单栏->文件->添加文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_File_AddFile_Click(object sender, RoutedEventArgs e) {
            var addFileWindow = new AddFile() {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Tags = tags.Where(x => !x.Group.StartsWith("Default - ")).ToList() ?? new List<TagInfo>(),
            };
            EventHandler<AddingTagEventArgs> eventHandler =
                (_sender, _e) => {
                    AddTag(_e.Tag);
                    var tagWithId = tags
                        .Where(item => item.Name == _e.Tag.Name && item.Group == _e.Tag.Group)
                        .First();
                    if (tagWithId != null) {
                        addFileWindow.AddTag(tagWithId);
                    }
                };
            addFileWindow.AddingTag += eventHandler;
            addFileWindow.ShowDialog();
            addFileWindow.AddingTag -= eventHandler;

            if (addFileWindow.IsOK) {
                foreach (var filename in addFileWindow.FileNames) {
                    AddFile(filename);
                    var oldTags = addFileWindow.SelectedTags.Where(x => x.Id != null).ToList();
                    var newTags = addFileWindow.SelectedTags.Where(x => x.Id == null).ToList();
                    newTags.ForEach(x => AddTag(x));
                    var newTagsWithId = tags.Join(newTags, x => new { x.Name, x.Group }, y => new { y.Name, y.Group }, (x, y) => x);
                    AddMapper(files.Where(x => x.Name == System.IO.Path.GetFileName(filename) && x.OriginalPath == System.IO.Path.GetDirectoryName(filename)).First()
                        , oldTags.Concat(newTagsWithId).ToArray());
                    TagTree_TagCheckChanged(null, new TagCheckChangedEventArgs(tagTree.SelectedTag.ToArray()));
                }
            }
        }

        /// <summary>
        /// 菜单栏->标签->添加标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_File_AddTag_Click(object sender, RoutedEventArgs e) {
            var addTagWindow = new AddTag() {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            addTagWindow.ShowDialog();
            if (addTagWindow.IsOK) {
                var tagInfo = new TagInfo() {
                    Name = addTagWindow.TagName,
                    Group = addTagWindow.TagGroup
                };
                AddTag(tagInfo);
            }
        }

        /// <summary>
        /// 菜单栏->文件->关闭按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_File_Close_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("真的要退出么！", "TagLibrary", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// 菜单栏->关于
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_File_About_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show($"TagLibrary\nVersion {version}\nCopyright © 2018 by LiRui");
        }

        /// <summary>
        /// 菜单栏->设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_File_Settings_Click(object sender, RoutedEventArgs e) {
            var settings = new Setting {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            settings.ShowDialog();
        }

        #endregion

        #region 文件列表事件处理方法

        /// <summary>
        /// 文件列表->右键->设置标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileList_ContextMenu_SetTag_Click(object sender, RoutedEventArgs e) {
            //var selectedFile = fileList.SelectedItems.Cast<FileInfo>();
            var file = fileList.SelectedItem as FileInfo;

            var setTagWindow = new SetTag() {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Tags = tags.Where(x => !x.Group.StartsWith("Default - ")).ToList() ?? new List<TagInfo>(),
                File = file,
                SelectedTags = mappers
                               .Where(x => x.FileId == (fileList.SelectedItem as FileInfo).Id)
                               .Join(tags, x => x.TagId, y => y.Id, (x, y) => y)
                               .Where(item => !item.Group.StartsWith("Default - "))
                               .ToList(),
            };
            EventHandler<AddingTagEventArgs> eventHandler =
                (_sender, _e) => {
                    AddTag(_e.Tag);
                    var tagWithId = tags
                        .Where(item => item.Name == _e.Tag.Name && item.Group == _e.Tag.Group)
                        .First();
                    if (tagWithId != null) {
                        setTagWindow.AddTag(tagWithId);
                    }
                };
            setTagWindow.AddingTag += eventHandler;
            setTagWindow.ShowDialog();
            setTagWindow.AddingTag -= eventHandler;
            if (setTagWindow.IsOK) {
                var oldTags = setTagWindow.SelectedTags.Where(x => x.Id != null).ToList();
                var newTags = setTagWindow.SelectedTags.Where(x => x.Id == null).ToList();
                newTags.ForEach(x => AddTag(x));
                var newTagsWithId = tags.Join(newTags, x => new { x.Name, x.Group }, y => new { y.Name, y.Group }, (x, y) => x);
                var allSelectedTags = oldTags.Concat(newTagsWithId);
                var needDelete =
                    mappers
                    .Where(item => item.FileId == file.Id)
                    .Join(tags, x => x.TagId, y => y.Id, (x, y) => y)
                    .Except(allSelectedTags)
                    .Where(item => !item.Group.StartsWith("Default - "))
                    .ToArray();

                AddMapper(file, oldTags.Concat(newTagsWithId).ToArray());
                RemoveMapper(file, needDelete);
                TagTree_TagCheckChanged(this, new TagCheckChangedEventArgs(tagTree.SelectedTag.ToArray()));

            }
        }

        /// <summary>
        /// 文件列表选择项变化时的处理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selectedFile = fileList.SelectedItems.Cast<FileInfo>();

        }

        #endregion

        #region 其他事件处理方法

        /// <summary>
        /// 事件->TagTree->选择Tag变化
        /// </summary>
        /// <param name="selectedTags"></param>
        private void TagTree_TagCheckChanged(object sender, TagCheckChangedEventArgs e) {
            if (e.Tags.Count() == 0) {
                (fileList.ItemsSource as BindingList<FileInfo>).Clear();
                foreach (var item in files) {
                    (fileList.ItemsSource as BindingList<FileInfo>).Add(item);
                }
            } else {
                (fileList.ItemsSource as BindingList<FileInfo>).Clear();
                e.Tags
                    .Join(mappers, x => x.Id, y => y.TagId, (x, y) => y.FileId)
                    .Join(files, x => x, y => y.Id, (x, y) => y)
                    .Distinct()
                    .ToList()
                    .ForEach(item => (fileList.ItemsSource as BindingList<FileInfo>).Add(item));
            }
        }

        #endregion

        /// <summary>
        /// 检查数据库版本
        /// </summary>
        private void CheckVersion() {
            if (db.DbMaintenance.IsAnyTable("TagLibrary")) {
                var result = db.Ado.SqlQueryDynamic("SELECT * FROM TagLibrary ORDER BY Major, Minor , Build DESC LIMIT 0, 1;");
                if (result[0].Major != versionMajor || result[0].Minor != versionMinor || result[0].Build != versionBuild) {
                    db.Ado.ExecuteCommand("INSERT INTO TagLibrary (Major, Minor, Build) VALUES (@Major, @Minor, @Build);"
                    , new List<SugarParameter>() {
                        new SugarParameter("Major", versionMajor),
                        new SugarParameter("Minor", versionMinor),
                        new SugarParameter("Build", versionBuild),
                    });
                }
                // 进行版本迁移操作
            } else {
                db.Ado.ExecuteCommand("CREATE TABLE IF NOT EXISTS TagLibrary (Major INT, Minor INT, Build INT);");
                db.Ado.ExecuteCommand("INSERT INTO TagLibrary (Major, Minor, Build) VALUES (@Major, @Minor, @Build);"
                    , new List<SugarParameter>() {
                        new SugarParameter("Major", versionMajor),
                        new SugarParameter("Minor", versionMinor),
                        new SugarParameter("Build", versionBuild),
                    });
            }
        }

        /// <summary>
        /// 根据实体生成数据库表
        /// </summary>
        private void CodeFirst() {
            if (!db.DbMaintenance.IsAnyTable("FileInfo")) {
                db.CodeFirst.InitTables(typeof(FileInfo));
            }
            if (!db.DbMaintenance.IsAnyTable("TagInfo")) {
                db.CodeFirst.InitTables(typeof(TagInfo));
                db.Insertable(new TagInfo() {
                    Name = "共享",
                    Group = "Default"
                });

            }
            if (!db.DbMaintenance.IsAnyTable("FileTagMapper")) {
                db.CodeFirst.InitTables(typeof(FileTagMapper));
            }
        }

        /// <summary>
        /// 添加文件
        /// </summary>
        /// <param name="fullFileName"></param>
        private void AddFile(string fullFileName) {
            FileInfo fileInfo = null;
            using (var fileStream = System.IO.File.OpenRead(fullFileName)) {
                fileInfo = new FileInfo() {
                    Name = System.IO.Path.GetFileName(fullFileName),
                    UUID = Guid.NewGuid().ToString().Replace("-", ""),
                    OriginalPath = System.IO.Path.GetDirectoryName(fullFileName),
                    Format = System.IO.Path.GetExtension(fullFileName),
                    Size = fileStream.Length
                };
            }
            System.IO.File.Copy(fullFileName, "library\\" + fileInfo.UUID + fileInfo.Format);
            fileInfo = db.Insertable(fileInfo).ExecuteReturnEntity();
            //(fileList.ItemsSource as BindingList<FileInfo>).Add(fileInfo);
            files.Add(fileInfo);

            //添加默认标签
            AddTag(new TagInfo() { Group = "Default - Format", Name = fileInfo.Format.ToUpper() });
            AddMapper(fileInfo, tags.Where(x => x.Group == "Default - Format" && x.Name == fileInfo.Format.ToUpper()).ToArray());
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileId"></param>
        private void RemoveFile(FileInfo fileInfo) {
            db.Deleteable<FileInfo>(fileInfo.Id).ExecuteCommand();
            files.Remove(fileInfo);
            (fileList.ItemsSource as BindingList<FileInfo>).Remove(fileInfo);
        }

        ///// <summary>
        ///// 添加主机
        ///// </summary>
        ///// <param name="host"></param>
        //private void AddHost(string host, string status = "offline") {
        //    if (hosts.Where(item => item.Host == host).Count() == 0) {
        //        hosts.Add(new HostInfo(host, status));
        //    } else {
        //        hosts.Where(item => item.Host == host).First().Status = status;
        //    }
        //}

        ///// <summary>
        ///// 移除主机
        ///// </summary>
        ///// <param name="host"></param>
        //private void RemoveHost(string host) {
        //    hosts.Remove(hosts.Where(item => item.Host == host).First());
        //}

        /// <summary>
        /// 添加Tag
        /// </summary>
        /// <param name="tagInfo"></param>
        private void AddTag(TagInfo tagInfo) {
            // 判断是否有重复TagInfo
            if (tags.Where(item => item.Name == tagInfo.Name && item.Group == tagInfo.Group).Count() == 0) {
                tagInfo = db.Insertable(tagInfo).ExecuteReturnEntity();
                tags.Add(tagInfo);
                tagTree.AddTag(new TagInfo[] { tagInfo });
            }
        }

        /// <summary>
        /// 删除Tag
        /// </summary>
        /// <param name="tagInfo"></param>
        private void RemoveTag(TagInfo tagInfo) {
            tags.Remove(tagInfo);
            db.Deleteable<TagInfo>(tagInfo.Id).ExecuteCommand();
        }

        /// <summary>
        /// 更新Tag
        /// </summary>
        /// <param name="tagInfo"></param>
        private void UpdateTag(TagInfo tagInfo) {
            var oldTag = tags.Find(item => item.Id == tagInfo.Id);
            if (oldTag != null) {
                oldTag.Name = tagInfo.Name ?? oldTag.Name;
                oldTag.Group = tagInfo.Group ?? oldTag.Group;
            }
        }

        /// <summary>
        /// 添加映射
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="tagInfos"></param>
        private void AddMapper(FileInfo fileInfo, TagInfo[] tagInfos) {
            //查找已映射TagInfo
            var notNeedAdd =
                mappers
                .Where(x => x.FileId == fileInfo.Id)
                .Select(x => x.TagId)
                .Join(tagInfos, x => x, y => y.Id, (x, y) => y);
            tagInfos.Except(notNeedAdd)
                .Select(x => new FileTagMapper() { FileId = fileInfo.Id ?? 0, TagId = x.Id ?? 0 })
                .ToList()
                .ForEach(x => mappers.Add(db.Insertable(x).ExecuteReturnEntity()));
        }

        /// <summary>
        /// 删除映射
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="tagInfos"></param>
        private void RemoveMapper(FileInfo fileInfo, TagInfo[] tagInfos = null) {
            if (tagInfos == null) {
                mappers
                .Where(item => item.FileId == fileInfo.Id)           //查找该文件所有mapper
                .Join(tags, x => x.TagId, y => y.Id, (x, y) => x)    //查找需要Remove的mapper
                .ToList().ForEach(item => { mappers.Remove(item); mappers.Remove(item); });
            } else {
                mappers
                    .Where(item => item.FileId == fileInfo.Id)               //查找该文件所有mapper
                    .Join(tagInfos, x => x.TagId, y => y.Id, (x, y) => x)    //查找需要Remove的mapper
                    .ToList().ForEach(item => { mappers.Remove(item); mappers.Remove(item); });
            }
        }

        private void TestData() {
            foreach (var file in System.IO.Directory.EnumerateFiles("library")) {
                System.IO.File.Delete(file);
            }

            #region Add File
            foreach (var file in System.IO.Directory.EnumerateFiles(@"D:\test")) {
                var fileInfo = new FileInfo() {
                    Name = System.IO.Path.GetFileName(file),
                    UUID = Guid.NewGuid().ToString().Replace("-", ""),
                    OriginalPath = System.IO.Path.GetDirectoryName(file),
                    Format = System.IO.Path.GetExtension(file).TrimStart('.').ToUpper(),
                    Size = System.IO.File.OpenRead(file).Length
                };
                db.Insertable(fileInfo).ExecuteCommand();
                System.IO.File.Copy(file, "library\\" + fileInfo.UUID + System.IO.Path.GetExtension(file));
            }
            #endregion
            #region Add Tag

            var groups = new List<string>();
            for (int i = 0; i < 3; i++) {
                groups.Add(Guid.NewGuid().ToString().Split('-')[1]);
            }

            for (int i = 0; i < 5; i++) {
                var tagInfo = new TagInfo() {
                    Name = Guid.NewGuid().ToString().Split('-')[1],
                    Group = groups[Guid.NewGuid().ToByteArray()[0] % 3]
                };
                db.Insertable(tagInfo).ExecuteCommand();
            }

            #endregion
            #region Add Mapper

            for (int i = 0; i < 10; i++) {
                db.Insertable(new FileTagMapper() {
                    FileId = Guid.NewGuid().ToByteArray()[8] % 15,
                    TagId = Guid.NewGuid().ToByteArray()[8] % 5
                }).ExecuteCommand();
            }

            #endregion
        }

        #region IDisposable Support
        private bool disposedValue = false;      //要检测冗余调用

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: 释放托管状态(托管对象)。
                    db.Dispose();
                    db = null;
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~MainWindow() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose() {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion

        private void FileList_ContextMenu_CopyTo_Click(object sender, RoutedEventArgs e) {
            var selectedFile = fileList.SelectedItems.Cast<FileInfo>().ToArray();

            var groups = selectedFile
                .Join(mappers, x => x.Id, y => y.FileId, (x, y) => new { file = x, y.TagId })
                .Join(tags, x => x.TagId, y => y.Id, (x, y) => new { fileId = (int) x.file.Id, tagGroup = y.Group })
                .Distinct()
                .GroupBy(item => item.tagGroup, (key, value) => new { tagGroup = key, count = value.Count() })
                .Where(item => item.count == selectedFile.Count())
                .Select(item => item.tagGroup);


            var copyToWindow = new CopyToFolder(groups.ToArray()) {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            copyToWindow.ShowDialog();
            if (!copyToWindow.IsOK) return;
            if (copyToWindow.SelectedGroup == "") {
                System.IO.Directory.CreateDirectory(copyToWindow.SelectedFolder + "\\TagLibrary");
                foreach (var file in selectedFile) {
                    try {
                        var filename = file.UUID + file.Format;
                        System.IO.File.Copy(
                            Environment.CurrentDirectory + @"\library\" + filename
                           , copyToWindow.SelectedFolder + "\\TagLibrary\\" + file.Name);
                    } catch { }
                }
            } else {
                System.IO.Directory.CreateDirectory(copyToWindow.SelectedFolder + "\\" + copyToWindow.SelectedGroup);
                selectedFile
                    .Join(mappers, x => x.Id, y => y.FileId, (x, y) => new { file = x, mapper = y })
                    .Join(tags, x => x.mapper.TagId, y => y.Id, (x, y) => new { x.file, tag = y })
                    .Where(item => item.tag.Group == copyToWindow.SelectedGroup)
                    .Distinct()
                    .ToList()
                    .ForEach(item => {
                        System.IO.Directory.CreateDirectory(copyToWindow.SelectedFolder + '\\' + item.tag.Group + '\\' + item.tag.Name);
                        System.IO.File.Copy(
                            Environment.CurrentDirectory + @"\library\" + item.file.UUID + item.file.Format,
                            copyToWindow.SelectedFolder + '\\' + item.tag.Group + '\\' + item.tag.Name + '\\' + item.file.Name);

                    });
            }
        }

        private void FileList_ContextMenu_Open_Click(object sender, RoutedEventArgs e) {
            if (fileList.SelectedItems.Count > 10) {
                if (MessageBox.Show("选择项大于10项，打开可能会耗费较多时间，真的要打开么？", "", MessageBoxButton.YesNo) == MessageBoxResult.No) {
                    return;
                }
            }
            foreach (var file in fileList.SelectedItems.Cast<FileInfo>()) {
                try {
                    var filename = Environment.CurrentDirectory + @"\library\" + file.UUID + file.Format;
                    var processStartInfo = new System.Diagnostics.ProcessStartInfo(filename);
                    var process = System.Diagnostics.Process.Start(processStartInfo);
                } catch { }
            }
        }

        private void FileList_ContextMenu_Delete_Click(object sender, RoutedEventArgs e) {
            fileList.SelectedItems
                .Cast<FileInfo>()
                .ToList()
                .ForEach(item => {
                    RemoveMapper(item);
                    RemoveFile(item);
                    System.IO.File.Delete(Environment.CurrentDirectory + @"\library\" + item.UUID + item.Format);
                });
        }
    }
}
