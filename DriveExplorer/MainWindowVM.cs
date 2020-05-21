using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DriveExplorer {
    public class MainWindowVM : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<TreeViewItemVM> treeViewItemVMs { get; set; } = new ObservableCollection<TreeViewItemVM>();

        public ObservableCollection<ListBoxItemVM> listBoxItemVMs { get; set; } = new ObservableCollection<ListBoxItemVM>();

        public MainWindowVM() {
            var drivePaths = Directory.GetLogicalDrives();
            foreach (var drivePath in drivePaths) {
                var model = new ItemModel(drivePath);
                treeViewItemVMs.Add(new TreeViewItemVM(model));
                listBoxItemVMs.Add(new ListBoxItemVM(model));
            }
        }

        public void TreeViewItem_Selected(object sender, RoutedEventArgs e = null) {
            var vm = (sender as TreeViewItemVM) ??
                (sender as TreeViewItem).DataContext as TreeViewItemVM ??
                throw new ArgumentException("invalid sender");
            // update list box items
            vm.IsExpanded = true; // in case there's no items as it hasn't been expanded 
            listBoxItemVMs.Clear();
            // add all folders
            foreach (var item in vm.Items) {
                listBoxItemVMs.Add(new ListBoxItemVM(item.Model));
            }
            // add all files
            var filePaths = Directory.GetFiles(vm.Model.FullPath);
            foreach (var filePath in filePaths) {
                listBoxItemVMs.Add(new ListBoxItemVM(filePath));
            }
            if (e != null) {
                e.Handled = true; // avoid recursive calls of treeViewItem.select
            }
        }
        public void ListBoxItem_Selected(object sender, MouseButtonEventArgs e = null) {
            var vm = (sender as ListBoxItemVM) ??
                (sender as ListBoxItem).DataContext as ListBoxItemVM ??
                throw new ArgumentException("invalid sender");
            // expand all treeViewItems
            var directories = vm.Model.FullPath.Split(Path.DirectorySeparatorChar)
                                         .Where(s => !string.IsNullOrEmpty(s));
            var parent = treeViewItemVMs;
            var child = TreeViewItemVM.Empty;
            foreach (var dir in directories) {
                child = parent.First(vm => vm.Model.Name == dir);
                child.IsExpanded = true;
                parent = child.Items;
            }
            child.IsSelected = true;
        }

        public void TreeViewItem_Selected(object sender, EventArgs e) {
            TreeViewItem_Selected(sender);
        }
        public void ListBoxItem_Selected(object sender, EventArgs e) {
            ListBoxItem_Selected(sender);
        }
    }
}
