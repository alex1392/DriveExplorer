using DriveExplorer.IoC;
using DriveExplorer.MicrosoftApi;
using Microsoft.Graph;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Directory = System.IO.Directory;

namespace DriveExplorer {
    public class MainWindowVM : INotifyPropertyChanged {
        private readonly AuthProvider authProvider;
        private readonly GraphManager graphManager;
        private readonly LocalItemFactory localItemFactory;
        private OneDriveItemFactory oneDriveItemFactory;

        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<ItemVM> ItemVMs { get; set; } = new ObservableCollection<ItemVM>();

        public ObservableCollection<ItemVM> listBoxItemVMs { get; set; } = new ObservableCollection<ItemVM>();

        public MainWindowVM(
            AuthProvider authProvider,
            GraphManager graphManager,
            LocalItemFactory localItemFactory,
            OneDriveItemFactory oneDriveItemFactory) {

            this.authProvider = authProvider;
            this.graphManager = graphManager;
            this.localItemFactory = localItemFactory;
            this.oneDriveItemFactory = oneDriveItemFactory;
            GetLocalDrives();
        }

        public void GetLocalDrives() {
            string[] drivePaths = null;
            try {
                drivePaths = Directory.GetLogicalDrives();
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show(ex.Message);
            }
            foreach (var drivePath in drivePaths) {
                var model = localItemFactory.Create(drivePath);
                ItemVMs.Add(new ItemVM(model));
                listBoxItemVMs.Add(new ItemVM(model));
            }
        }

        public async Task GetOneDriveAsync() {
            var root = await graphManager.GetDriveRootAsync();
            var item = new ItemVM(oneDriveItemFactory.Create(root));
            ItemVMs.Add(item);
        }

        public async Task TreeViewItem_SelectedAsync(object sender, RoutedEventArgs e = null) {
            var vm = (sender as ItemVM) ??
                (sender as TreeViewItem).DataContext as ItemVM ??
                throw new ArgumentException("invalid sender");
            if (e != null) {
                e.Handled = true; // avoid recursive calls of treeViewItem.select
            }
            // update list box items
            await vm.ExpandAsync(); // in case there's no items as it hasn't been expanded 
            listBoxItemVMs.Clear();
            foreach (var itemVM in vm.Children) {
                listBoxItemVMs.Add(new ItemVM(itemVM.Item));
            }

        }
        public async Task ListBoxItem_SelectedAsync(object sender, MouseButtonEventArgs e = null) {
            var vm = (sender as ItemVM) ??
                (sender as ListBoxItem).DataContext as ItemVM ??
                throw new ArgumentException("invalid sender");
            if (!vm.Item.Type.Is(ItemTypes.Folders)) {
                return;
            }
            // expand all treeViewItems
            var directories = vm.Item.FullPath.Split(Path.DirectorySeparatorChar).Where(s => !string.IsNullOrEmpty(s));
            var parent = ItemVMs;
            var child = ItemVM.Empty;
            foreach (var dir in directories) {
                child = parent.First(vm => vm.Item.Name == dir);
                await child.ExpandAsync();
                parent = child.Children;
            }
            child.IsSelected = true;
        }
    }
}
