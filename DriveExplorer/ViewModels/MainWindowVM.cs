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
		private readonly string localRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(DriveExplorer));
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
		public MainWindowVM(ILogger logger = null, MicrosoftManager microsoftManager = null, GoogleManager googleManager = null) {
			this.logger = logger;
			this.microsoftManager = microsoftManager;
			this.googleManager = googleManager;
			if (this.logger == null) {
				Console.WriteLine($"{typeof(ILogger)} is not attached to {this.GetType()}.");
			}
			if (this.microsoftManager == null) {
				Console.WriteLine($"{typeof(MicrosoftManager)} is not attached to {this.GetType()}.");
			} else {
				this.microsoftManager.BeforeTaskExecute += (_,_) => ShowSpinner();
				this.microsoftManager.TaskExecuted += (_, _) => HideSpinner();
			}
			if (this.googleManager == null) {
				Console.WriteLine($"{typeof(GoogleManager)} is not attached to {this.GetType()}.");
			} else {
				this.googleManager.BeforeTaskExecute += (_, _) => ShowSpinner();
				this.googleManager.TaskExecuted += (_, _) => HideSpinner();
			}
		}

		/// <summary>
		/// Check if local root folder is created, this method should be called when the application startups.
		/// </summary>
		public void SetupLocalRoot() {
			if (!Directory.Exists(localRootPath)) {
				Directory.CreateDirectory(localRootPath);
			}
		}
		/// <summary>
		/// Attach all local drives to <see cref="TreeItemVMs"/>. This method should be called at the startup of application.
		/// </summary>
		public void GetLocalDrives() {
			string[] drivePaths = null;
			try {
				drivePaths = Directory.GetLogicalDrives();
			} catch (UnauthorizedAccessException ex) {
				logger?.Log(ex);
			}
			foreach (var drivePath in drivePaths) {
				var item = new LocalItem(drivePath);
				AddTreeItemVM(item);
			}
		}
		public async Task TreeItemSelectedAsync(object sender, RoutedEventArgs e = null) {
			var itemVM = (sender as ItemVM) ??
				(sender as TreeViewItem).DataContext as ItemVM ??
				throw new ArgumentException("invalid sender");
			if (e != null) {
				e.Handled = true; // avoid recursive calls of treeViewItem.select
			}
			// update list box items, in case there's no items as it hasn't been expanded 
			await itemVM.SetIsExpandedAsync(true).ConfigureAwait(true);
			CurrentItemVMs.Clear();
			foreach (var childVM in itemVM.Children) {
				CurrentItemVMs.Add(childVM.Clone());
			}
		}
		public async Task CurrentItemSelectedAsync(object sender, MouseButtonEventArgs e = null) {
			var vm = (sender as ItemVM) ??
				(sender as ListBoxItem).DataContext as ItemVM ??
				throw new ArgumentException("invalid sender");
			if (vm.Item.Type.Is(ItemTypes.Folders)) {
				await CurrentItemFolderSelectedAsync(vm);
			} else if (vm.Item.Type.Is(ItemTypes.Files)) {

			} else {
				
			}
		}

		/// <summary>
		/// Reset <see cref="TreeItemVMs"/> and <see cref="CurrentItemVMs"/>.
		/// </summary>
		public void Reset() {
			TreeItemVMs.Clear();
			CurrentItemVMs.Clear();
		}
		private void CurrentItemFileSelected(ItemVM vm) {
			// check if the file has been cached

			// if true, open the cached file with default application

			// if false, download the file to cache

		}

		private async Task CurrentItemFolderSelectedAsync(ItemVM vm) {
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
		}

		private void ShowSpinner() {
			SpinnerVisibility = Visibility.Visible;
		}

		private void HideSpinner() {
			SpinnerVisibility = Visibility.Collapsed;
		}

		private void AddTreeItemVM(IItem item) {
			var itemVM = new ItemVM(item, localRootPath);
			itemVM.BeforeExpand += (_, _) => ShowSpinner();
			itemVM.Expanded += (_, _) => HideSpinner();
			TreeItemVMs.Add(itemVM);
		}

		#region MicrosoftApi
		public async Task LoginOneDriveAsync() {
			if (microsoftManager == null) {
				return;
			}
			var result = await microsoftManager.LoginInteractively().ConfigureAwait(true);
			await CreateOneDriveAsync(result?.Account).ConfigureAwait(false);
		}
		/// <summary>
		/// TODO: not auto login at launch, just retrieve account cache, and login when user want to access the drive
		/// </summary>
		/// <returns></returns>
		public async Task AutoLoginOneDriveAsync() {
			if (microsoftManager == null) {
				return;
			}
			await foreach (var result in microsoftManager.LoginAllUserSilently().ConfigureAwait(true)) {
				await CreateOneDriveAsync(result?.Account).ConfigureAwait(true);
			}
		}
		public async Task LogoutOneDriveAsync(ItemVM treeVM) {
			if (treeVM.Item.Type != ItemTypes.OneDrive) {
				throw new InvalidOperationException();
			}
			if (microsoftManager == null) {
				return;
			}
			if (treeVM == null) {
				logger?.Log("User has been logged out");
			} else {
				var item = treeVM.Item as OneDriveItem;
				if (await microsoftManager.LogoutAsync(item.UserAccount).ConfigureAwait(true)) {
					TreeItemVMs.Remove(treeVM);
				}
			}
		}
		private async Task CreateOneDriveAsync(IAccount account) {
			if (microsoftManager == null) {
				return;
			}
			if (account is null) {
				return;
			}
			if (TreeItemVMs.Any(vm => vm.Item.Name == account.Username)) {
				logger?.Log("User has already signed in.");
				return;
			}
			var root = await microsoftManager.GetDriveRootAsync(account).ConfigureAwait(true);
			if (root == null) {
				return;
			}
			var item = new OneDriveItem(microsoftManager, root, account);
			AddTreeItemVM(item);
		}

		
		#endregion

		#region GoogleApi
		public async Task LoginGoogleDriveAsync() {
			if (googleManager == null) {
				return;
			}
			//SwitchSpinner(); // dot not show spinner, let the task expired automatically
			var userId = await googleManager.UserLoginAsync().ConfigureAwait(true);
			await CreateGoogleDrive(userId).ConfigureAwait(true);
			//SwitchSpinner();
		}
		public async Task AutoLoginGoogleDriveAsync() {
			if (googleManager == null) {
				return;
			}
			foreach (var userId in googleManager.LoadAllUserId()) {
				await googleManager.UserLoginAsync(userId).ConfigureAwait(true);
				await CreateGoogleDrive(userId).ConfigureAwait(true);
			}
		}
		private async Task CreateGoogleDrive(string userId) {
			var about = await googleManager.GetAboutAsync(userId).ConfigureAwait(true);
			if (about == null) {
				return;
			}
			if (TreeItemVMs.Any(vm => vm.Item.Name == about.User.EmailAddress)) {
				logger?.Log("User has already logged in.");
				return;
			}
			var root = await googleManager.GetDriveRootAsync(userId).ConfigureAwait(true);
			if (root == null) {
				return;
			}
			var item = new GoogleDriveItem(googleManager, about, root, userId);
			AddTreeItemVM(item);
		}
		public async Task LogoutGoogleDriveAsync(ItemVM treeVM) {
			if (googleManager == null) {
				return;
			}
			if (!(treeVM.Item is GoogleDriveItem googleDriveItem)) {
				return;
			}
			if (await googleManager.UserLogoutAsync(googleDriveItem.UserId).ConfigureAwait(true)) {
				TreeItemVMs.Remove(treeVM);
			}
		}
		#endregion
	}
}
