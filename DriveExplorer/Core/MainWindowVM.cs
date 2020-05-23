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

        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<ItemVM> ItemVMs { get; set; } = new ObservableCollection<ItemVM>();

        public ObservableCollection<ItemVM> listBoxItemVMs { get; set; } = new ObservableCollection<ItemVM>();

        public MainWindowVM() {
            string[] drivePaths = null;
            try {
                drivePaths = Directory.GetLogicalDrives();
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show(ex.Message);
            }
            foreach (var drivePath in drivePaths) {
                var model = IocContainer.Default.GetSingleton<LocalItemFactory>().Create(drivePath);
                var item = new ItemVM(model);
                ItemVMs.Add(item);
                listBoxItemVMs.Add(item);
            }
        }

        [PreferredConstructor]
        public MainWindowVM(AuthProvider authProvider, GraphManager graphManager) : this() {
            this.authProvider = authProvider;
            this.graphManager = graphManager;
            var _ =Task.Run(() => authProvider.GetAccessToken()).Result;
            var root = Task.Run(() => graphManager.GetDriveRootAsync()).Result;
            var item = new ItemVM(IocContainer.Default.GetSingleton<OneDriveItemFactory>().Create(root));
            ItemVMs.Add(item);
        }

        public async Task LoginOneDriveAsync() {
            var root = await graphManager.GetDriveRootAsync();
            var item = new ItemVM(IocContainer.Default.GetSingleton<OneDriveItemFactory>().Create(root));
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
            foreach (var item in vm.Children) {
                listBoxItemVMs.Add(item);
            }

        }
        public async Task ListBoxItem_SelectedAsync(object sender, MouseButtonEventArgs e = null) {
            var vm = (sender as ItemVM) ??
                (sender as ListBoxItem).DataContext as ItemVM ??
                throw new ArgumentException("invalid sender");
            if (ItemTypes.Folder.Add(ItemTypes.Drive).Contains(vm.Item.Type)) {
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
