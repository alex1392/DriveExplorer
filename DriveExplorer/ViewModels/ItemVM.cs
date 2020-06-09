using DataVirtualization;
using DriveExplorer.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DriveExplorer.ViewModels {

	public class ItemVM : INotifyPropertyChanged {

		#region Private Fields

		private bool haveExpanded = false;
		private bool isExpanded;
		private bool isSelected;

		#endregion Private Fields

		#region Public Events

		public event EventHandler BeforeExpand;

		public event EventHandler BeforeSelect;

		public event EventHandler Expanded;

		public event PropertyChangedEventHandler PropertyChanged;

		public event EventHandler Selected;

		#endregion Public Events

		#region Public Properties


		public bool IsLocal => CacheRootPath == null;

		public string CacheFullPath { get; private set; }

		public string CacheRootPath { get; private set; }

		public VirtualizingCollection<ItemVM> Children { get; set; }

		public ObservableCollection<ItemVM> SubFolders { get; set; } = new ObservableCollection<ItemVM>
		{
			null,
		};

		public bool IsCached {
			get {
				// check if cache file exist, and the last modified date and file size is matched
				if (Item.ItemType.IsMember(ItemTypes.Folders)) {
					return Directory.Exists(CacheFullPath);
				} else if (Item.ItemType == ItemTypes.File) {
					var info = new FileInfo(CacheFullPath);
					return info.Exists &&
						info.Length == Item.Size &&
						info.LastWriteTimeUtc == Item.LastModifiedTime;
				} else {
					throw new InvalidOperationException();
				}
			}
		}

		/// <summary>
		/// Change the state of expansion and invoke <see cref="ExpandAsync"/> without await. To expand item programmically, call <see cref="SetIsExpandedAsync(bool)"/> instead.
		/// </summary>
		public bool IsExpanded {
			get => isExpanded;
			set {
				if (value != isExpanded) {
					isExpanded = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
				}
				if (value == true) {
					ExpandAsync();
				}
			}
		}

		/// <summary>
		/// Change the state of selection and invoke <see cref="SelectAsync"/> without await. To select item programmically, call <see cref="SetIsSelectedAsync(bool)"/> instead.
		/// </summary>
		public bool IsSelected {
			get => isSelected;
			set {
				if (value != IsSelected) {
					isSelected = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
				}
				if (value == true) {
					SelectAsync();
				}
			}
		}

		public IItem Item { get; private set; }
		public ItemVMChildrenProvider ChildrenProvider { get; }
		public ItemVM Parent { get; }
		public ImageSource Icon { get; private set; }

		#endregion Public Properties

		#region Public Constructors
		/// <summary>
		/// For xaml design time
		/// </summary>
		public ItemVM()
		{

		}

		/// <summary>
		/// Root Constructor
		/// </summary>
		public ItemVM(IItem item, string localRootPath)
		{
			if (item.ItemType == ItemTypes.File) {
				throw new TypeInitializationException(nameof(ItemVM), new Exception("File cannot be set as a root of a tree view."));
			}
			Item = item;
			if (item.DriveType != DriveTypes.LocalDrive) {
				CacheRootPath = localRootPath;
				CacheFullPath = Path.Combine(CacheRootPath, Item.ItemType.ToString(), Item.FullPath);
			} else {
				CacheFullPath = Item.FullPath;
			}
			CacheFolder();
			SetIcon();
			ChildrenProvider = new ItemVMChildrenProvider(this);
			Children = new VirtualizingCollection<ItemVM>(ChildrenProvider, 10);
		}

		/// <summary>
		/// Child constructor
		/// </summary>
		public ItemVM(IItem item, ItemVM parent)
		{
			Item = item;
			Parent = parent;
			if (!parent.IsLocal) {
				CacheRootPath = parent.CacheRootPath;
			}
			CacheFullPath = Path.Combine(parent.CacheFullPath, Item.Name);
			// inherit parent's events
			BeforeExpand += parent.BeforeExpand;
			Expanded += parent.Expanded;

			if (Item.ItemType.IsMember(ItemTypes.Folders)) {
				CacheFolder();
				ChildrenProvider = new ItemVMChildrenProvider(this);
				Children = new VirtualizingCollection<ItemVM>(ChildrenProvider, 10);
			}
			SetIcon();
		}

		#endregion Public Constructors

		#region Public Methods

		public async Task CacheFileAsync()
		{
			if (IsCached) {
				return;
			}
			await Item.DownloadAsync(CacheFullPath).ConfigureAwait(false);
			// set file info
			File.SetLastWriteTimeUtc(CacheFullPath, Item.LastModifiedTime.Value.LocalDateTime);
		}

		/// <summary>
		/// Set <see cref="IsExpanded"/> asynchronizly.
		/// </summary>
		public async Task SetIsExpandedAsync(bool value)
		{
			await ExpandAsync().ConfigureAwait(false);
			if (value != isExpanded) {
				isExpanded = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
			}
		}

		/// <summary>
		/// Set <see cref="IsSelected"/> asynchronizly
		/// </summary>
		public async Task SetIsSelectedAsync(bool value)
		{
			await SelectAsync().ConfigureAwait(false);
			if (value != isSelected) {
				isSelected = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
			}
		}

		public override string ToString()
		{
			return $"{Item.Name}, Items: {Children.Count}";
		}

		#endregion Public Methods

		#region Private Methods
		private void SetIcon()
		{
			if (Item.ItemType == ItemTypes.Drive) {
				Icon = new BitmapImage(new Uri($"pack://application:,,,/DriveExplorer;component/Resources/{Item.DriveType}.png"));
			} else if (Item.ItemType == ItemTypes.Folder) {
				Icon = new BitmapImage(new Uri($"pack://application:,,,/DriveExplorer;component/Resources/{Item.ItemType}.png"));
			} else if (IsCached) {
				var icon = System.Drawing.Icon.ExtractAssociatedIcon(CacheFullPath);
				Icon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			} else {
				Icon = new BitmapImage(new Uri($"pack://application:,,,/DriveExplorer;component/Resources/file.png"));
			}
		}
		private void CacheFolder()
		{
			if (IsLocal) {
				return;
			}
			if (!IsCached) {
				Directory.CreateDirectory(CacheFullPath);
			}
		}

		private async Task ExpandAsync()
		{
			if (!Item.ItemType.IsMember(ItemTypes.Folders)) {
				return;
			}
			if (haveExpanded) {
				return;
			}
			BeforeExpand?.Invoke(this, null);
			await DoExpand();
			Expanded?.Invoke(this, null);

			async Task DoExpand()
			{
				haveExpanded = true;
				if (HasDummyItem) {
					SubFolders.Clear(); // clear dummy item
				}
				// attach children
				await foreach (var item in Item.GetSubFolderAsync().ConfigureAwait(true)) {
					// item with the same name is not allowed
					if (!SubFolders.Any(vm => vm.Item.Name == item.Name)) {
						SubFolders.Add(new ItemVM(item, this));
					}
				}
				if (!IsLocal) {
					// delete any folder is not consistent with cloud
					Directory.GetDirectories(CacheFullPath)
						.Where(path => !SubFolders.Any(vm => vm.CacheFullPath == path))
						.ToList()
						.ForEach(path => Directory.Delete(path, recursive: true));
				}
			}
		}
		public bool HasDummyItem => SubFolders.Count == 1 && SubFolders[0] == null;

		private async Task SelectAsync()
		{
			BeforeSelect?.Invoke(this, null);

			if (Parent != null && !Parent.HasDummyItem) {
				var itemVM = Parent.SubFolders.FirstOrDefault(vm =>
					   vm.Item.Name == this.Item.Name &&
					   vm != this);
				if (itemVM != null) {
					await itemVM.SetIsSelectedAsync(true);
				}
			}
			//await Task.CompletedTask;

			Selected?.Invoke(this, null);
		}

		#endregion Private Methods
	}
}