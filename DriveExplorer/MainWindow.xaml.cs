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

namespace DriveExplorer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private static readonly object dummyItem = new object();

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            string[] drives;
            try {
                drives = Directory.GetLogicalDrives();
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show(ex.Message);
                return;
            }
            foreach (var drivePath in drives) {
                Item item = new Item
                {
                    Type = Item.Types.Drive,
                    Name = drivePath.Replace(Path.DirectorySeparatorChar, ' ').Trim(),
                    FullPath = drivePath,
                };
                TreeViewItem driveItem = new TreeViewItem
                {
                    DataContext = item,
                };
                driveItem.Items.Add(dummyItem); // add dummyItem for the expansion indicator
                driveItem.Expanded += FolderItem_Expand;
                driveItem.Selected += FolderItem_Selected;
                FolderTreeView.Items.Add(driveItem);
                ItemView.Items.Add(item);
            }

        }

        private void FolderItem_Expand(object sender, RoutedEventArgs e) {
            if (!(sender is TreeViewItem treeViewItem) ||
                !(treeViewItem.DataContext is Item item)) {
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
                    DataContext = new Item
                    {
                        Type = Item.Types.Folder,
                        Name = Path.GetFileName(subfolderPath),
                        FullPath = subfolderPath,
                    },
                };
                subfolderItem.Items.Add(dummyItem); // add dummy item for expansion indicator
                subfolderItem.Expanded += FolderItem_Expand;
                subfolderItem.Selected += FolderItem_Selected;
                treeViewItem.Items.Add(subfolderItem);
            }
        }

        private void FolderItem_Selected(object sender, RoutedEventArgs e) {
            if (!(sender is TreeViewItem treeViewItem) ||
                 !(treeViewItem.DataContext is Item item)) {
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
                Item subfolderItem = new Item()
                {
                    Type = Item.Types.Folder,
                    Name = Path.GetFileName(subfolderPath),
                    FullPath = subfolderPath,
                };
                ItemView.Items.Add(subfolderItem);
            }
            foreach (var filePath in filePaths) {
                Item.Types type;
                switch (Path.GetExtension(filePath).ToLower()) {
                    case ".jpg":
                        type = Item.Types.IMG; break;
                    case ".jpeg":
                        type = Item.Types.IMG; break;
                    case ".png":
                        type = Item.Types.IMG; break;
                    case ".bmp":
                        type = Item.Types.IMG; break;
                    case ".gif":
                        type = Item.Types.IMG; break;
                    case ".txt":
                        type = Item.Types.TXT; break;
                    case ".doc":
                        type = Item.Types.DOC; break;
                    case ".docx":
                        type = Item.Types.DOC; break;
                    case ".xls":
                        type = Item.Types.XLS; break;
                    case ".xlsx":
                        type = Item.Types.XLS; break;
                    case ".ppt":
                        type = Item.Types.PPT; break;
                    case ".pptx":
                        type = Item.Types.PPT; break;
                    case ".rar":
                        type = Item.Types.ZIP; break;
                    case ".zip":
                        type = Item.Types.ZIP; break;
                    default:
                        type = Item.Types.File; break;
                }
                ItemView.Items.Add(new Item()
                {
                    Type = type,
                    Name = Path.GetFileName(filePath),
                    FullPath = filePath,
                });
            }
            e.Handled = true; // prevent recursive selection behavior of treeview
        }

        private void Item_Selected(object sender, RoutedEventArgs e) {
            if (!(sender is ListBoxItem listBoxItem) ||
                 !(listBoxItem.DataContext is Item item)) {
                throw new ArgumentException("invalid folder item");
            }
            if (item.Type != Item.Types.Folder &&
                item.Type != Item.Types.Drive) {
                // open file with default processor
                return;
            }
            // expand tree view folder
            string fullPath = item.FullPath;
            var paths = fullPath.Split(Path.DirectorySeparatorChar).Where(s => !string.IsNullOrEmpty(s)).ToList();
            var treeView = FolderTreeView as ItemsControl;
            foreach (var path in paths) {
                var treeViewItem = treeView.Items.Cast<TreeViewItem>().First(treeViewItem =>
                    (treeViewItem.DataContext as Item ?? throw new Exception()).Name == path);
                (treeView.ItemContainerGenerator.ContainerFromItem(treeViewItem) as TreeViewItem).IsExpanded = true;
                treeView = treeViewItem;
            }
            (treeView as TreeViewItem ?? throw new Exception()).IsSelected = true;
        }
    }

    public class Item {
        public enum Types {
            Folder,
            Drive,
            File,
            IMG,
            TXT,
            DOC,
            XLS,
            PPT,
            ZIP,
        }
        public Item() {
        }

        public Item(Types type, string name, string fullPath) {
            Type = type;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
        }

        public Types Type { get; set; }

        public string Name { get; set; }
        public string FullPath { get; set; }
    }

}
