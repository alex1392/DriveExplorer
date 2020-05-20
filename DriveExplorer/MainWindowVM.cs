using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DriveExplorer {
    public class MainWindowVM : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<TreeViewItemVM> treeViewItems { get; set; } = new ObservableCollection<TreeViewItemVM>();

        public ObservableCollection<ListBoxItemVM> listBoxItems { get; set; } = new ObservableCollection<ListBoxItemVM>();

        public MainWindowVM() {
            var drivePaths = Directory.GetLogicalDrives();
            foreach (var drivePath in drivePaths) {
                var model = new ItemModel(ItemModel.Types.Drive, drivePath, true);
                treeViewItems.Add(new TreeViewItemVM(model));
                listBoxItems.Add(new ListBoxItemVM(model));
            }
        }

        internal void TreeViewItem_Expanded(object sender, RoutedEventArgs e) {
            if (!(sender is TreeViewItem treeViewItem) ||
                !(treeViewItem.DataContext is TreeViewItemVM vm)) {
                Console.WriteLine("invalid sender");
                return;
            }
            vm.Expand();
        }
        internal void TreeViewItem_Selected(object sender, RoutedEventArgs e) {
            if (!(sender is TreeViewItem treeViewItem) ||
                !(treeViewItem.DataContext is TreeViewItemVM vm)) {
                Console.WriteLine("invalid sender");
                return;
            }
            // update list box items
            vm.Expand(); // in case there's no items as it hasn't been expanded 
            listBoxItems.Clear();
            foreach (var item in vm.Items) {
                listBoxItems.Add(new ListBoxItemVM(item.Model));
            }
            e.Handled = true; // avoid recursive calls of treeViewItem.select
        }
        internal void ListBoxItem_Selected(object sender, MouseButtonEventArgs e) {
            if (!(sender is ListBoxItem listBoxItem) || 
                !(listBoxItem.DataContext is ListBoxItemVM vm)) {
                Console.WriteLine("invalid sender");
                return;
            }
            // expand all treeViewItems
            var directories = vm.Model.FullPath.Split(Path.DirectorySeparatorChar)
                                         .Where(s => !string.IsNullOrEmpty(s));
            var parent = treeViewItems;
            var child = TreeViewItemVM.Empty;
            foreach (var dir in directories) {
                child = parent.First(vm => vm.Model.Name == dir);
                child.IsExpanded = true;
                parent = child.Items;
            }
            child.IsSelected = true;
        }

    }
}
