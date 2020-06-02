using Cyc.Standard;

using DriveExplorer.Models;

using System;
using System.Collections.Generic;
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
		private readonly NavigationManager<ItemVM> navigationManager;
		private readonly IDriveManager googleDriveManager;
		private readonly LocalDriveManager localDriveManager;
		private readonly ILogger logger;
		private readonly IDriveManager oneDriveManager;
		private CancellationTokenSource currentCancellationTokenSource;
		private Visibility spinnerVisibility = Visibility.Collapsed;


		#endregion Private Fields

		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion Public Events

		#region Public Properties
		public ObservableCollection<ItemVM> CurrentItemVMs => CurrentFolder?.Children;

		public List<ItemVM> PathItemVMs {
			get {
				var list = new List<ItemVM>();
				var vm = CurrentFolder;
				while (vm != null) {
					list.Add(vm);
					vm = vm.Parent;
				}
				list.Reverse();
				return list;
			}
		}

		/// <summary>
		/// Should always be the selected <see cref="ItemVM"/>
		/// </summary>
		public ItemVM CurrentFolder {
			get => navigationManager.Current;
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

		public ICommand PreviousPageCommand { get; private set; }
		public ICommand NextPageCommand { get; private set; }
		public ICommand ParentFolderCommand { get; private set; }
		public bool IsBusy { get; private set; }

		#endregion Public Properties

		#region Public Constructors

		public MainWindowVM(
			IDispatcher dispatcher,
			NavigationManager<ItemVM> navigationManager,
			ILogger logger = null,
			LocalDriveManager localDriveManager = null,
			OneDriveManager oneDriveManager = null,
			GoogleDriveManager googleDriveManager = null)
		{
			this.logger = logger;
			this.dispatcher = dispatcher;
			this.navigationManager = navigationManager;
			this.localDriveManager = localDriveManager;
			this.oneDriveManager = oneDriveManager;
			this.googleDriveManager = googleDriveManager;
			
			PreviousPageCommand = new PreviousPageCommand(navigationManager);
			NextPageCommand = new NextPageCommand(navigationManager);
			ParentFolderCommand = new ParentFolderCommand(navigationManager);

			navigationManager.CurrentChanged += (_, _) => {
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFolder)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentItemVMs)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PathItemVMs)));
			};

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
			IsBusy = true;
			if (e != null) {
				e.Handled = true; // avoid recursive calls of treeViewItem.select
			}
			await itemVM.SetIsExpandedAsync(true).ConfigureAwait(true);
			navigationManager.Add(itemVM);
			IsBusy = false;
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