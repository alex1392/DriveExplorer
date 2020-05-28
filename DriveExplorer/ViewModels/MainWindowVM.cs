using DriveExplorer.MicrosoftApi;
using DriveExplorer.Models;
using Microsoft.Graph;
using Microsoft.Identity.Client;
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

namespace DriveExplorer.ViewModels {
	public class MainWindowVM : INotifyPropertyChanged {
		private readonly GraphManager graphManager;
		private Visibility spinnerVisibility = Visibility.Collapsed;

		public event PropertyChangedEventHandler PropertyChanged;
		public ObservableCollection<ItemVM> TreeItemVMs { get; } = new ObservableCollection<ItemVM>();

		public ObservableCollection<ItemVM> CurrentItemVMs { get; } = new ObservableCollection<ItemVM>();

		public Visibility SpinnerVisibility {
			get => spinnerVisibility;
			set {
				if (value != spinnerVisibility) {
					spinnerVisibility = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpinnerVisibility)));
				}
			}
		}

		public MainWindowVM(GraphManager graphManager) {
			this.graphManager = graphManager;
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
				var item = new LocalItem(drivePath);
				TreeItemVMs.Add(new ItemVM(item));
				CurrentItemVMs.Add(new ItemVM(item));
			}
		}
		public async Task LoginOneDrive() {
			SpinnerVisibility = Visibility.Visible;
			var (_, account) = await graphManager.GetAccessTokenInteractively().ConfigureAwait(true);
			await CreateOneDriveAsync(account).ConfigureAwait(false);
			SpinnerVisibility = Visibility.Collapsed;
		}
		public async Task AutoLoginOneDrive() {
			SpinnerVisibility = Visibility.Visible;
			await foreach (var (_, account) in graphManager.GetAllAccessTokenSilently().ConfigureAwait(true)) {
				await CreateOneDriveAsync(account).ConfigureAwait(true);
			}
			SpinnerVisibility = Visibility.Collapsed;
		}
		public async Task LogoutOneDriveAsync() {
			SpinnerVisibility = Visibility.Visible;
			// TODO: logout specific user
			var treeVM = TreeItemVMs.FirstOrDefault(vm => vm.Item.Type == ItemTypes.OneDrive);
			if (treeVM == null) {
				MessageBox.Show("User has been logged out");
			} else {
				var item = treeVM.Item as OneDriveItem;
				if (await graphManager.LogoutAsync(item.UserAccount).ConfigureAwait(true)) {
					TreeItemVMs.Remove(treeVM);
					CurrentItemVMs.Remove(CurrentItemVMs.FirstOrDefault(vm => vm == treeVM));
				}
			}
			SpinnerVisibility = Visibility.Collapsed;
		}
		private async Task CreateOneDriveAsync(IAccount account) {
			if (account is null) {
				return;
			}
			if (TreeItemVMs.Any(vm => vm.Item.Name == account.Username)) {
				// TODO: change it to a logger
				MessageBox.Show("User has already signed in.");
				return;
			}
			var root = await graphManager.GetDriveRootAsync(account).ConfigureAwait(true);
			if (root == null) {
				return;
			}
			var item = new OneDriveItem(graphManager, root, account);
			TreeItemVMs.Add(new ItemVM(item, this));
			CurrentItemVMs.Add(new ItemVM(item, this));
		}

		public async Task TreeItem_SelectedAsync(object sender, RoutedEventArgs e = null) {
			var vm = (sender as ItemVM) ??
				(sender as TreeViewItem).DataContext as ItemVM ??
				throw new ArgumentException("invalid sender");
			if (e != null) {
				e.Handled = true; // avoid recursive calls of treeViewItem.select
			}
			SpinnerVisibility = Visibility.Visible;
			// update list box items, in case there's no items as it hasn't been expanded 
			await vm.SetIsExpandedAsync(true).ConfigureAwait(true);
			CurrentItemVMs.Clear();
			foreach (var itemVM in vm.Children) {
				CurrentItemVMs.Add(new ItemVM(itemVM.Item, this));
			}
			SpinnerVisibility = Visibility.Collapsed;
		}
		public async Task CurrentItem_SelectedAsync(object sender, MouseButtonEventArgs e = null) {
			var vm = (sender as ItemVM) ??
				(sender as ListBoxItem).DataContext as ItemVM ??
				throw new ArgumentException("invalid sender");
			if (!vm.Item.Type.Is(ItemTypes.Folders)) {
				return;
			}

			SpinnerVisibility = Visibility.Visible;
			// expand all treeViewItems
			var directories = vm.Item.FullPath.Split(Path.DirectorySeparatorChar).Where(s => !string.IsNullOrEmpty(s));
			var parent = TreeItemVMs;
			ItemVM lastChild = null;
			foreach (var dir in directories) {
				var child = parent.FirstOrDefault(vm => vm.Item.Name == Uri.UnescapeDataString(dir)) ?? throw new Exception("Cannot find folder to expand");
				await child.SetIsExpandedAsync(true).ConfigureAwait(true);
				parent = child.Children;
				lastChild = child;
			}
			await lastChild.SetIsSelectedAsync(true).ConfigureAwait(true);
			SpinnerVisibility = Visibility.Collapsed;
		}

		/// <summary>
		/// Reset <see cref="TreeItemVMs"/> and <see cref="CurrentItemVMs"/>.
		/// </summary>
		public void Reset() {
			TreeItemVMs.Clear();
			CurrentItemVMs.Clear();
			spinnerVisibility = Visibility.Collapsed;
		}
	}
}
