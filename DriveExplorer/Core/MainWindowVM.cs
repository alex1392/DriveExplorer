using DriveExplorer.MicrosoftApi;
using Microsoft.Graph;
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

		public event PropertyChangedEventHandler PropertyChanged;
		public ObservableCollection<ItemVM> TreeItemVMs { get; } = new ObservableCollection<ItemVM>();

		public ObservableCollection<ItemVM> CurrentItemVMs { get; } = new ObservableCollection<ItemVM>();

		public MainWindowVM(AuthProvider authProvider, GraphManager graphManager) {
			this.authProvider = authProvider;
			this.graphManager = graphManager;
			// must not call GetOneDriveAsync in the constructor of MainWindowVM, 
			// as if user uses ioc to create MainWindowVM, the thread passes through syncLock in ioc, while here it tries to create OneDriveItem within the constructor, which causes another thread tring to get into the syncLock, which has been held by the former thread. Consequently, a deadlock occurs.
			// Task.Run(async () => await GetOneDriveAsync()).Wait();
		}

		/// <summary>
		/// Attach all local drives to <see cref="TreeItemVMs"/>. This method should be called at the startup of application.
		/// </summary>
		public void GetLocalDrives() {
			string[] drivePaths = null;
			try {
				drivePaths = Directory.GetLogicalDrives();
			} catch (UnauthorizedAccessException ex) {
				MessageBox.Show(ex.Message);
			}
			foreach (var drivePath in drivePaths) {
				var model = LocalItemFactory.Create(drivePath);
				TreeItemVMs.Add(new ItemVM(model));
			}
		}
		public async Task LoginOneDrive() {
			var token = await authProvider.GetAccessTokenInteractively().ConfigureAwait(true);
			if (token == null) {
				return;
			}
			await CreateOneDriveAsync().ConfigureAwait(false);

		}
		public async Task AutoLoginOneDrive() {
			var token = await authProvider.GetAccessToken().ConfigureAwait(true);
			if (token == null) {
				return;
			}
			await CreateOneDriveAsync().ConfigureAwait(false);

		}
		public async Task LogoutOneDriveAsync() {
			var treeVM = TreeItemVMs.First(vm => vm.Item.Type == ItemTypes.OneDrive);
			var item = treeVM.Item as OneDriveItem;
			if (await authProvider.LogoutAsync(item.UserId).ConfigureAwait(true)) {
				TreeItemVMs.Remove(treeVM);
				CurrentItemVMs.Remove(CurrentItemVMs.FirstOrDefault(vm => vm == treeVM));
			}
		}
		private async Task CreateOneDriveAsync() {
			var userAccount = authProvider.CurrentUserAccount;
			if (TreeItemVMs.Any(vm => vm.Item.Name == userAccount.Username)) {
				MessageBox.Show("User has already signed in.");
				return;
			}
			var user = await graphManager.GetMeAsync().ConfigureAwait(true);
			if (user == null) {
				return;
			}
			if (authProvider.UserIdAccountRegistry.ContainsKey(user.Id)) {
				MessageBox.Show("User has already signed in.");
				return;
			}
			var root = await graphManager.GetDriveRootAsync().ConfigureAwait(true);
			if (root == null) {
				return;
			}

			authProvider.UserIdAccountRegistry.Add(user.Id, userAccount);
			var item = new ItemVM(OneDriveItemFactory.CreateRoot(root, user));
			TreeItemVMs.Add(item);
		}

		/// <summary>
		/// Set up start page of <see cref="CurrentItemVMs"/>. This method should be called after all drives have been attached to <see cref="TreeItemVMs"/>.
		/// </summary>
		public void StartPage() {
			foreach (var itemVM in TreeItemVMs) {
				CurrentItemVMs.Add(new ItemVM(itemVM.Item));
			}
		}

		public async Task TreeItem_SelectedAsync(object sender, RoutedEventArgs e = null) {
			var vm = (sender as ItemVM) ??
				(sender as TreeViewItem).DataContext as ItemVM ??
				throw new ArgumentException("invalid sender");
			if (e != null) {
				e.Handled = true; // avoid recursive calls of treeViewItem.select
			}
			// update list box items
			await vm.SetIsExpandedAsync(true).ConfigureAwait(true); // in case there's no items as it hasn't been expanded 
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
				child = parent.FirstOrDefault(vm => vm.Item.Name == Uri.UnescapeDataString(dir)) ?? throw new Exception("Cannot find folder to expand");
				await child.SetIsExpandedAsync(true).ConfigureAwait(true);
				parent = child.Children;
			}
			await child.SetIsSelectedAsync(true).ConfigureAwait(true);
		}

		/// <summary>
		/// Reset <see cref="TreeItemVMs"/> and <see cref="CurrentItemVMs"/>.
		/// </summary>
		public void Reset() {
			TreeItemVMs.Clear();
			CurrentItemVMs.Clear();
		}
	}
}
