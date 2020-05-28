using Cyc.GoogleApi;
using Cyc.MicrosoftApi;
using Cyc.Standard;
using DriveExplorer.Models;

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
		private readonly ILogger logger;
		private readonly MicrosoftManager microsoftManager;
		private readonly GoogleManager googleManager;
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

		public MainWindowVM(ILogger logger, MicrosoftManager microsoftManager, GoogleManager googleManager) {
			this.logger = logger;
			this.microsoftManager = microsoftManager;
			this.googleManager = googleManager;
		}
		/// <summary>
		/// Attach all local drives to <see cref="TreeItemVMs"/>. This method should be called at the startup of application.
		/// </summary>
		public void GetLocalDrives() {
			string[] drivePaths = null;
			try {
				drivePaths = Directory.GetLogicalDrives();
			} catch (UnauthorizedAccessException ex) {
				logger.Log(ex);
			}
			foreach (var drivePath in drivePaths) {
				var item = new LocalItem(drivePath);
				TreeItemVMs.Add(new ItemVM(item));
				CurrentItemVMs.Add(new ItemVM(item));
			}
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

		#region MicrosoftApi

		public async Task LoginOneDriveAsync() {
			if (microsoftManager == null) {
				Console.WriteLine("microsoftManager is not attached to mainwindowVM");
				return;
			}
			SpinnerVisibility = Visibility.Visible;
			var (_, account) = await microsoftManager.GetAccessTokenInteractively().ConfigureAwait(true);
			await CreateOneDriveAsync(account).ConfigureAwait(false);
			SpinnerVisibility = Visibility.Collapsed;
		}
		/// <summary>
		/// TODO: not auto login at launch, just retrieve account cache, and login when user want to access the drive
		/// </summary>
		/// <returns></returns>
		public async Task AutoLoginOneDriveAsync() {
			if (microsoftManager == null) {
				Console.WriteLine("microsoftManager is not attached to mainwindowVM");
				return;
			}
			SpinnerVisibility = Visibility.Visible;
			await foreach (var (_, account) in microsoftManager.GetAllAccessTokenSilently().ConfigureAwait(true)) {
				await CreateOneDriveAsync(account).ConfigureAwait(true);
			}
			SpinnerVisibility = Visibility.Collapsed;
		}
		public async Task LogoutOneDriveAsync() {
			if (microsoftManager == null) {
				Console.WriteLine("microsoftManager is not attached to mainwindowVM");
				return;
			}
			SpinnerVisibility = Visibility.Visible;
			// TODO: logout specific user
			var treeVM = TreeItemVMs.FirstOrDefault(vm => vm.Item.Type == ItemTypes.OneDrive);
			if (treeVM == null) {
				logger.Log("User has been logged out");
			} else {
				var item = treeVM.Item as OneDriveItem;
				if (await microsoftManager.LogoutAsync(item.UserAccount).ConfigureAwait(true)) {
					TreeItemVMs.Remove(treeVM);
					CurrentItemVMs.Remove(CurrentItemVMs.FirstOrDefault(vm => vm == treeVM));
				}
			}
			SpinnerVisibility = Visibility.Collapsed;
		}
		private async Task CreateOneDriveAsync(IAccount account) {
			if (microsoftManager == null) {
				Console.WriteLine("microsoftManager is not attached to mainwindowVM");
				return;
			}
			if (account is null) {
				return;
			}
			if (TreeItemVMs.Any(vm => vm.Item.Name == account.Username)) {
				logger.Log("User has already signed in.");
				return;
			}
			var root = await microsoftManager.GetDriveRootAsync(account).ConfigureAwait(true);
			if (root == null) {
				return;
			}
			var item = new OneDriveItem(microsoftManager, root, account);
			TreeItemVMs.Add(new ItemVM(item, this));
			CurrentItemVMs.Add(new ItemVM(item, this));
		}

		#endregion

		#region GoogleApi
		public async Task LoginGoogleDriveAsync() {
			var about = await googleManager.GetAboutAsync().ConfigureAwait(true);
			var root = await googleManager.GetDriveRootAsync().ConfigureAwait(true);
			var item = new GoogleDriveItem(googleManager, about, root);
			TreeItemVMs.Add(new ItemVM(item));
			CurrentItemVMs.Add(new ItemVM(item));
		}
		#endregion
	}
}
