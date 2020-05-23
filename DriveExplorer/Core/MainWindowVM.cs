using DriveExplorer.MicrosoftApi;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        public ObservableCollection<ItemVM> TreeItemVMs { get; set; } = new ObservableCollection<ItemVM>();

        public ObservableCollection<ItemVM> CurrentItemVMs { get; set; } = new ObservableCollection<ItemVM>();

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

            // must create the second IoC object within the same thread of the first creating object, otherwise the syncLock would be held by the first thread, the second thread would be block until the first thread is finished. However, the first object need the second object in order to complete its creation. Consequently, a deadlock occurs.
            var root = Task.Run(async () => await graphManager.GetDriveRootAsync()).Result;
            var item = new ItemVM(oneDriveItemFactory.Create(root)); 
            TreeItemVMs.Add(item);
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
                TreeItemVMs.Add(new ItemVM(model));
                CurrentItemVMs.Add(new ItemVM(model));
            }
        }
        
        public async Task GetOneDriveAsync() {
            var root = await graphManager.GetDriveRootAsync();
            var item = new ItemVM(oneDriveItemFactory.Create(root));
            TreeItemVMs.Add(item);
        }

        public async Task TreeItem_SelectedAsync(object sender, RoutedEventArgs e = null) {
            var vm = (sender as ItemVM) ??
                (sender as TreeViewItem).DataContext as ItemVM ??
                throw new ArgumentException("invalid sender");
            if (e != null) {
                e.Handled = true; // avoid recursive calls of treeViewItem.select
            }
            // update list box items
            await vm.ExpandAsync(); // in case there's no items as it hasn't been expanded 
            CurrentItemVMs.Clear();
            foreach (var itemVM in vm.Children) {
                CurrentItemVMs.Add(new ItemVM(itemVM.Item));
            }

        }
        public async Task CurrentItem_SelectedAsync(object sender, MouseButtonEventArgs e = null) {
            var vm = (sender as ItemVM) ??
                (sender as ListBoxItem).DataContext as ItemVM ??
                throw new ArgumentException("invalid sender");
            if (!vm.Item.Type.Is(ItemTypes.Folders)) {
                return;
            }
            // expand all treeViewItems
            var directories = vm.Item.FullPath.Split(Path.DirectorySeparatorChar).Where(s => !string.IsNullOrEmpty(s));
            var parent = TreeItemVMs;
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
