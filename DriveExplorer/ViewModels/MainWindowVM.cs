using Cyc.Standard;

using DriveExplorer.Models;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DriveExplorer.ViewModels {

	public class MainWindowVM : INotifyPropertyChanged {

		#region Private Fields

		private readonly string CacheRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(DriveExplorer));

		private readonly IDispatcher dispatcher;
		private readonly IDriveManager googleDriveManager;
		private readonly LocalDriveManager localDriveManager;
		private readonly ILogger logger;
		private readonly IDriveManager oneDriveManager;
		private CancellationTokenSource currentCancellationTokenSource;
		private ItemVM currentFolder = null;
		private Visibility spinnerVisibility = Visibility.Collapsed;

		#endregion Private Fields

		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion Public Events

		#region Public Properties

		public ItemVM CurrentFolder {
			get => currentFolder;
			private set {
				if (currentFolder != value) {
					currentFolder = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFolder)));
				}
			}
		}

		public Visibility SpinnerVisibility {
			get => spinnerVisibility;
			set {
				if (value != spinnerVisibility) {
					spinnerVisibility = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpinnerVisibility)));
				}
			}
		}

		public ObservableCollection<ItemVM> TreeItemVMs { get; } = new ObservableCollection<ItemVM>();

		#endregion Public Properties

		#region Public Constructors

		public MainWindowVM(
			IDispatcher dispatcher,
			ILogger logger = null,
			LocalDriveManager localDriveManager = null,
			OneDriveManager oneDriveManager = null,
			GoogleDriveManager googleDriveManager = null)
		{
			this.logger = logger;
			this.dispatcher = dispatcher;
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

		#endregion Public Constructors

		#region Public Methods

		public void CancelCurrentTask()
		{
			currentCancellationTokenSource?.Cancel();
		}

		public async Task CurrentItemSelectedAsync(object sender, MouseButtonEventArgs e = null)
		{
			var vm = (sender as ItemVM) ??
				(sender as ListBoxItem).DataContext as ItemVM ??
				throw new ArgumentException("invalid sender");
			if (vm.Item.Type.Is(ItemTypes.Folders)) {
				await CurrentItemFolderSelectedAsync(vm).ConfigureAwait(false);
			} else if (vm.Item.Type == ItemTypes.File) {
				await CurrentItemFileSelectedAsync(vm).ConfigureAwait(false);
			} else {
				throw new InvalidOperationException();
			}
		}

		public async Task InitializeAsync()
		{
			// check local cache
			if (!Directory.Exists(CacheRootPath)) {
				Directory.CreateDirectory(CacheRootPath);
			}
			if (localDriveManager != null) {
				await localDriveManager.AutoLoginAsync().ConfigureAwait(true);
			}
			if (oneDriveManager != null) {
				await oneDriveManager.AutoLoginAsync().ConfigureAwait(true);
			}
			if (googleDriveManager != null) {
				await googleDriveManager.AutoLoginAsync().ConfigureAwait(true);
			}
		}

		public async Task LoginGoogleDriveAsync()
		{
			if (googleDriveManager == null) {
				return;
			}
			currentCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));
			await googleDriveManager.LoginAsync(currentCancellationTokenSource.Token).ConfigureAwait(false);
		}

		public async Task LoginOneDriveAsync()
		{
			if (oneDriveManager == null) {
				return;
			}
			await oneDriveManager.LoginAsync().ConfigureAwait(false);
		}

		public async Task LogoutGoogleDriveAsync(IItem item)
		{
			if (googleDriveManager == null) {
				return;
			}
			await googleDriveManager.LogoutAsync(item).ConfigureAwait(false);
		}

		public async Task LogoutOneDriveAsync(IItem item)
		{
			if (oneDriveManager == null) {
				return;
			}
			await oneDriveManager.LogoutAsync(item).ConfigureAwait(false);
		}

		public void Reset()
		{
			TreeItemVMs.Clear();
		}

		public async Task TreeItemSelectedAsync(object sender, RoutedEventArgs e = null)
		{
			var itemVM = (sender as ItemVM) ??
				(sender as TreeViewItem).DataContext as ItemVM ??
				throw new ArgumentException("invalid sender");
			if (e != null) {
				e.Handled = true; // avoid recursive calls of treeViewItem.select
			}
			await itemVM.SetIsExpandedAsync(true).ConfigureAwait(true);
			CurrentFolder = itemVM;
		}

		#endregion Public Methods

		#region Private Methods

		private void AddTreeItemVM(IItem item)
		{
			var itemVM = new ItemVM(item, CacheRootPath);
			itemVM.BeforeExpand += (_, _) => ShowSpinner();
			itemVM.Expanded += (_, _) => HideSpinner();
			dispatcher?.Invoke(() => {
				TreeItemVMs.Add(itemVM);
			});
		}

		private async Task CurrentItemFileSelectedAsync(ItemVM vm)
		{
			// check if the file has been cached
			if (!vm.IsCached) {
				// download the file to cache
				await vm.CacheFileAsync().ConfigureAwait(false);
			}
			// open the cached file with default application
			try {
				new Process
				{
					StartInfo = new ProcessStartInfo(vm.CacheFullPath)
					{
						UseShellExecute = true,
					}
				}.Start();
			} catch (Win32Exception ex) {
				logger?.Log(ex);
			}
		}

		private async Task CurrentItemFolderSelectedAsync(ItemVM vm)
		{
			await vm.SetIsSelectedAsync(true).ConfigureAwait(true);
			// expand all ancestor treeViewItems
			var directories = vm.Item.FullPath
				.Split(Path.DirectorySeparatorChar)
				.Where(s => !string.IsNullOrEmpty(s));
			while (vm != null) {
				await vm.SetIsExpandedAsync(true).ConfigureAwait(true);
				vm = vm.Parent;
			}
		}

		private void HideSpinner()
		{
			dispatcher?.Invoke(() => {
				SpinnerVisibility = Visibility.Collapsed;
			});
		}

		private void RemoveTreeItemVM(IItem item)
		{
			// every account must be unique
			var vm = TreeItemVMs.FirstOrDefault(vm => vm.Item.Name == item.Name) ?? throw new InvalidOperationException();
			dispatcher?.Invoke(() => {
				TreeItemVMs.Remove(vm);
			});
		}

		private void ShowSpinner()
		{
			dispatcher?.Invoke(() => {
				SpinnerVisibility = Visibility.Visible;
			});
		}

		#endregion Private Methods
	}
}