using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace TreeViewTest {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Window1 : Window {
        private static readonly object dummyItem = new object();

        public Window1() {
            InitializeComponent();
        }

        public void Window_Loaded(object sender, RoutedEventArgs e) {
            string[] drives;
            try {
                drives = Directory.GetLogicalDrives();
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show(ex.Message);
                return;
            }
            foreach (var drivePath in drives) {
                ItemModel item = new ItemModel
                {
                    Type = ItemModel.Types.Drive,
                    Name = drivePath.Replace(Path.DirectorySeparatorChar, ' ').Trim(),
                    FullPath = drivePath,
                };
                TreeViewItem driveItem = new TreeViewItem
                {
                    DataContext = item,
                };
                driveItem.Items.Add(dummyItem); // add dummyItem for the expansion indicator
                FolderTreeView.Items.Add(driveItem);
                ItemView.Items.Add(item);
            }

        }

        public void FolderItem_Expand(object sender, RoutedEventArgs e) {
            if (!(sender is TreeViewItem treeViewItem) ||
                !(treeViewItem.DataContext is ItemModel item)) {
                throw new ArgumentException("invalid folder item");
            }
            // have already been expanded
            if (treeViewItem.Items.Count != 1 ||
                treeViewItem.Items[0] != dummyItem) {
                return;
            }
            treeViewItem.Items.Clear(); // remove dummy item
            var folderPath = item.FullPath;
            string[] subfolderPaths;
            try {
                subfolderPaths = Directory.GetDirectories(folderPath);
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show(ex.Message);
                return;
            }

            foreach (var subfolderPath in subfolderPaths) {
                TreeViewItem subfolderItem = new TreeViewItem
                {
                    DataContext = new ItemModel
                    {
                        Type = ItemModel.Types.Folder,
                        Name = Path.GetFileName(subfolderPath),
                        FullPath = subfolderPath,
                    },
                };
                subfolderItem.Items.Add(dummyItem); // add dummy item for expansion indicator
                treeViewItem.Items.Add(subfolderItem);
            }
        }

        public void FolderItem_Selected(object sender, RoutedEventArgs e) {
            if (!(sender is TreeViewItem treeViewItem) ||
                 !(treeViewItem.DataContext is ItemModel item)) {
                throw new ArgumentException("invalid folder item");
            }
            treeViewItem.IsExpanded = true;
            var folderPath = item.FullPath;
            string[] filePaths;
            string[] subfolderPaths;
            try {
                filePaths = Directory.GetFiles(folderPath);
                subfolderPaths = Directory.GetDirectories(folderPath);
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show(ex.Message);
                return;
            }

            ItemView.Items.Clear();
            foreach (var subfolderPath in subfolderPaths) {
                ItemModel subfolderItem = new ItemModel()
                {
                    Type = ItemModel.Types.Folder,
                    Name = Path.GetFileName(subfolderPath),
                    FullPath = subfolderPath,
                };
                ItemView.Items.Add(subfolderItem);
            }
            foreach (var filePath in filePaths) {
                ItemModel.Types type;
                switch (Path.GetExtension(filePath).ToLower()) {
                    case ".jpg":
                        type = ItemModel.Types.IMG; break;
                    case ".jpeg":
                        type = ItemModel.Types.IMG; break;
                    case ".png":
                        type = ItemModel.Types.IMG; break;
                    case ".bmp":
                        type = ItemModel.Types.IMG; break;
                    case ".gif":
                        type = ItemModel.Types.IMG; break;
                    case ".txt":
                        type = ItemModel.Types.TXT; break;
                    case ".doc":
                        type = ItemModel.Types.DOC; break;
                    case ".docx":
                        type = ItemModel.Types.DOC; break;
                    case ".xls":
                        type = ItemModel.Types.XLS; break;
                    case ".xlsx":
                        type = ItemModel.Types.XLS; break;
                    case ".ppt":
                        type = ItemModel.Types.PPT; break;
                    case ".pptx":
                        type = ItemModel.Types.PPT; break;
                    case ".rar":
                        type = ItemModel.Types.ZIP; break;
                    case ".zip":
                        type = ItemModel.Types.ZIP; break;
                    default:
                        type = ItemModel.Types.File; break;
                }
                ItemView.Items.Add(new ItemModel()
                {
                    Type = type,
                    Name = Path.GetFileName(filePath),
                    FullPath = filePath,
                });
            }
            e.Handled = true; // prevent recursive selection behavior of treeview
        }

        public void Item_Selected(object sender, RoutedEventArgs e) {
            if (!(sender is ListBoxItem listBoxItem) ||
                 !(listBoxItem.DataContext is ItemModel item)) {
                throw new ArgumentException("invalid folder item");
            }
            if (item.Type != ItemModel.Types.Folder &&
                item.Type != ItemModel.Types.Drive) {
                // open file with default processor
                return;
            }
            // expand tree view folder
            string fullPath = item.FullPath;
            var paths = fullPath.Split(Path.DirectorySeparatorChar).Where(s => !string.IsNullOrEmpty(s)).ToList();
            var treeView = FolderTreeView as ItemsControl;
            foreach (var path in paths) {
                var treeViewItem = treeView.Items.Cast<TreeViewItem>().First(treeViewItem =>
                    (treeViewItem.DataContext as ItemModel).Name == path);
                (treeView.ItemContainerGenerator.ContainerFromItem(treeViewItem) as TreeViewItem).IsExpanded = true;
                treeView = treeViewItem;
            }
            (treeView as TreeViewItem).IsSelected = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            new MainWindow().Show();
        }
    }

}
