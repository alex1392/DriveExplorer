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

	public class ItemVM : INotifyPropertyChanged, IEquatable<ItemVM> {

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

		public ObservableCollection<ItemVM> Children { get; private set; } = new ObservableCollection<ItemVM>
		{
			null // add dummyItem for the expansion indicator
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
		public ItemVM Parent { get; }
		public ImageSource Icon { get; private set; }

		#endregion Public Properties

		#region Public Constructors

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
		}

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
			}
			SetIcon();
		}

		#endregion Public Constructors

		#region Public Methods

		public static bool operator !=(ItemVM left, ItemVM right)
		{
			return !(left == right);
		}

		public static bool operator ==(ItemVM left, ItemVM right)
		{
			return EqualityComparer<ItemVM>.Default.Equals(left, right);
		}

		public async Task CacheFileAsync()
		{
			if (IsCached) {
				return;
			}
			await Item.DownloadAsync(CacheFullPath).ConfigureAwait(false);
			// set file info
			File.SetLastWriteTimeUtc(CacheFullPath, Item.LastModifiedTime.Value.LocalDateTime);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ItemVM);
		}

		public bool Equals(ItemVM other)
		{
			return other != null &&
				Item.Name == other.Item.Name &&
				Item.ItemType == other.Item.ItemType &&
				Item.FullPath == other.Item.FullPath;
		}

		public override int GetHashCode()
		{
			var hashCode = 424228742;
			hashCode = hashCode * -1521134295 + isExpanded.GetHashCode();
			hashCode = hashCode * -1521134295 + haveExpanded.GetHashCode();
			hashCode = hashCode * -1521134295 + isSelected.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<IItem>.Default.GetHashCode(Item);
			hashCode = hashCode * -1521134295 + EqualityComparer<ObservableCollection<ItemVM>>.Default.GetHashCode(Children);
			return hashCode;
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

		/// <summary>
		/// Attach certain child without expand it
		/// </summary>
		public ItemVM AttachChild(IItem item)
		{
			if (HasDummyItem) {
				Children.Clear();
			}
			var itemVM = new ItemVM(item, this);
			Children.Add(itemVM);
			return itemVM;
		}

		public override string ToString()
		{
			return $"{Item.Name}, Items: {Children.Count}";
		}

		#endregion Public Methods

		#region Private Methods

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
					Children.Clear(); // clear dummy item
				}
				// attach children
				await foreach (var item in Item.GetChildrenAsync().ConfigureAwait(true)) {
					// item with the same name is not allowed
					if (!Children.Any(vm => vm.Item.Name == item.Name)) {
						Children.Add(new ItemVM(item, this));
					}
				}
				if (!IsLocal) {
					// delete any folder is not consistent with cloud
					Directory.GetDirectories(CacheFullPath)
						.Where(path => !Children.Any(vm => vm.CacheFullPath == path))
						.ToList()
						.ForEach(path => Directory.Delete(path, recursive: true));
				}
			}
		}
		public bool HasDummyItem => Children.Count == 1 && Children[0] == null;

		private async Task SelectAsync()
		{
			BeforeSelect?.Invoke(this, null);

			await Task.CompletedTask;

			Selected?.Invoke(this, null);
		}

		#endregion Private Methods
	}
}