using Cyc.GoogleApi;
using Cyc.MicrosoftApi;
using Cyc.Standard;

using DriveExplorer.Models;

using Microsoft.Identity.Client;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Directory = System.IO.Directory;
namespace DriveExplorer.ViewModels {
	public class MainWindowVM : INotifyPropertyChanged {
		private readonly string localRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(DriveExplorer));

		private readonly ILogger logger;
		private readonly LocalDriveManager localDriveManager;
		private readonly OneDriveManager oneDriveManager;
		private readonly GoogleDriveManager googleDriveManager;
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


		public MainWindowVM(ILogger logger = null,
			LocalDriveManager localDriveManager = null,
			OneDriveManager oneDriveManager = null,
			GoogleDriveManager googleDriveManager = null)
		{
			this.logger = logger;
			this.localDriveManager = localDriveManager;
			this.oneDriveManager = oneDriveManager;
			this.googleDriveManager = googleDriveManager;
			SetupDriveManagers();

			void SetupDriveManagers()
			{
				if (this.logger == null) {
					Console.WriteLine($"{typeof(ILogger)} is not attached to {this.GetType()}.");
				}
				if (this.localDriveManager == null) {
					Console.WriteLine($"{typeof(LocalDriveManager)} is not attached to {this.GetType()}.");
				} else {
					localDriveManager.LoginCompleted += (_, item) => AddTreeItemVM(item);
					localDriveManager.LogoutCompleted += (_, item) => RemoveTreeItemVM(item);
				}
				if (this.oneDriveManager == null) {
					Console.WriteLine($"{typeof(OneDriveManager)} is not attached to {this.GetType()}.");
				} else {
					oneDriveManager.LoginCompleted += (_, item) => AddTreeItemVM(item);
					oneDriveManager.LogoutCompleted += (_, item) => RemoveTreeItemVM(item);
					oneDriveManager.BeforeTaskExecuted
						+= (_, _) => ShowSpinner();
					oneDriveManager.TaskExecuted
						+= (_, _) => HideSpinner();
				}
				if (this.googleDriveManager == null) {
					Console.WriteLine($"{typeof(GoogleDriveManager)} is not attached to {this.GetType()}.");
				} else {
					googleDriveManager.LoginCompleted += (_, item) => AddTreeItemVM(item);
					googleDriveManager.LogoutCompleted += (_, item) => RemoveTreeItemVM(item);
					googleDriveManager.BeforeTaskExecuted += (_, _) => ShowSpinner();
					googleDriveManager.TaskExecuted += (_, _) => HideSpinner();
				}
			}
		}

		public async Task InitializeAsync()
		{
			// check local cache
			if (!Directory.Exists(localRootPath)) {
				Directory.CreateDirectory(localRootPath);
			}
			await localDriveManager.AutoLoginAsync().ConfigureAwait(true);
			await oneDriveManager.AutoLoginAsync().ConfigureAwait(true);
			await googleDriveManager.AutoLoginAsync().ConfigureAwait(true);
		}
		public async Task TreeItemSelectedAsync(object sender, RoutedEventArgs e = null)
		{
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
		public async Task CurrentItemSelectedAsync(object sender, MouseButtonEventArgs e = null)
		{
			var vm = (sender as ItemVM) ??
				(sender as ListBoxItem).DataContext as ItemVM ??
				throw new ArgumentException("invalid sender");
			if (vm.Item.Type.Is(ItemTypes.Folders)) {
				await CurrentItemFolderSelectedAsync(vm).ConfigureAwait(false);
			} else if (vm.Item.Type.Is(ItemTypes.Files)) {
				await CurrentItemFileSelectedAsync(vm).ConfigureAwait(false);
			} else {
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Reset <see cref="TreeItemVMs"/> and <see cref="CurrentItemVMs"/>.
		/// </summary>
		public void Reset()
		{
			TreeItemVMs.Clear();
			CurrentItemVMs.Clear();
		}
		public async Task LoginOneDriveAsync()
		{
			await oneDriveManager.LoginAsync().ConfigureAwait(false);
		}

		public async Task LogoutOneDriveAsync(IItem item)
		{
			await oneDriveManager.LogoutAsync(item).ConfigureAwait(false);
		}
		public async Task LoginGoogleDriveAsync()
		{
			await googleDriveManager.LoginAsync().ConfigureAwait(false);
		}
		public async Task LogoutGoogleDriveAsync(IItem item)
		{
			await googleDriveManager.LogoutAsync(item).ConfigureAwait(false);
		}

		private async Task CurrentItemFileSelectedAsync(ItemVM vm)
		{
			if (vm.Item.Type.Is(ItemTypes.Files)) {
				// check if the file has been cached
				if (!vm.IsCached) {
					// download the file to cache
					await vm.CacheFileAsync().ConfigureAwait(false);
				}
				// open the cached file with default application
				Process.Start(vm.CacheFullPath);
			}
		}

		private async Task CurrentItemFolderSelectedAsync(ItemVM vm)
		{
			// expand all treeViewItems
			var directories = vm.Item.FullPath
				.Split(Path.DirectorySeparatorChar)
				.Where(s => !string.IsNullOrEmpty(s));
			var treeVM = vm.LinkedVM;
			while (treeVM != null) {
				await treeVM.SetIsSelectedAsync(true).ConfigureAwait(true);
				treeVM = treeVM.Parent;
			}
		}

		private void ShowSpinner()
		{
			Application.Current.Dispatcher.Invoke(() => {
				SpinnerVisibility = Visibility.Visible;
			});
		}

		private void HideSpinner()
		{
			Application.Current.Dispatcher.Invoke(() => {
				SpinnerVisibility = Visibility.Collapsed;
			});
		}

		private void AddTreeItemVM(IItem item)
		{
			var itemVM = new ItemVM(item, localRootPath);
			itemVM.BeforeExpand += (_, _) => ShowSpinner();
			itemVM.Expanded += (_, _) => HideSpinner();
			Application.Current.Dispatcher.Invoke(() => {
				TreeItemVMs.Add(itemVM);
			});
		}
		private void RemoveTreeItemVM(IItem item)
		{
			var vm = TreeItemVMs.FirstOrDefault(vm => vm.Item.Name == item.Name) ?? throw new InvalidOperationException();
			Application.Current.Dispatcher.Invoke(() => {
				TreeItemVMs.Remove(vm);
			});
		}


	}
}
