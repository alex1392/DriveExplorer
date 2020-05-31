using DriveExplorer.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

		public string CacheFullPath { get; private set; }

		public string CacheRootPath { get; private set; }

		public ObservableCollection<ItemVM> Children { get; private set; } = new ObservableCollection<ItemVM>
		{
			null // add dummyItem for the expansion indicator
        };

		public bool IsCached {
			get {
				// check if cache file exist, and the last modified date and file size is matched
				if (Item.Type.Is(ItemTypes.Folders)) {
					return Directory.Exists(CacheFullPath);
				} else if (Item.Type == ItemTypes.File) {
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

		#endregion Public Properties

		#region Public Constructors

		/// <summary>
		/// Root Constructor
		/// </summary>
		public ItemVM(IItem item, string localRootPath)
		{
			if (!item.Type.Is(ItemTypes.Drives)) {
				throw new TypeInitializationException(nameof(ItemVM), null);
			}
			Item = item;
			CacheRootPath = localRootPath;
			CacheFullPath = Path.Combine(CacheRootPath, Item.Type.ToString(), Item.FullPath);
			CacheFolder();
		}

		/// <summary>
		/// Child constructor
		/// </summary>
		public ItemVM(IItem item, ItemVM parent)
		{
			Item = item;
			Parent = parent;
			CacheRootPath = parent.CacheRootPath;
			CacheFullPath = Path.Combine(parent.CacheFullPath, Item.Name);
			// inherit parent's events
			BeforeExpand += parent.BeforeExpand;
			Expanded += parent.Expanded;

			if (Item.Type.Is(ItemTypes.Folders)) {
				CacheFolder();
			}
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
				Item.Type == other.Item.Type &&
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
			if (value != isExpanded) {
				isExpanded = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
			}
			await ExpandAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Set <see cref="IsSelected"/> asynchronizly
		/// </summary>
		public async Task SetIsSelectedAsync(bool value)
		{
			if (value != isSelected) {
				isSelected = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
			}
			await SelectAsync().ConfigureAwait(false);
		}

		public override string ToString()
		{
			return $"{Item.Name}, Items: {Children.Count}";
		}

		#endregion Public Methods

		#region Private Methods

		private void CacheFolder()
		{
			if (CacheRootPath == null) {
				return;
			}
			if (!IsCached) {
				Directory.CreateDirectory(CacheFullPath);
			}
		}

		private async Task ExpandAsync()
		{
			if (!Item.Type.Is(ItemTypes.Folders)) {
				return;
			}
			// attach children
			if (haveExpanded) {
				return;
			}
			BeforeExpand?.Invoke(this, null);
			await DoExpand();
			Expanded?.Invoke(this, null);

			async Task DoExpand()
			{
				haveExpanded = true;
				Children.Clear(); // clear dummy item
				await foreach (var item in Item.GetChildrenAsync().ConfigureAwait(true)) {
					Children.Add(new ItemVM(item, this));
				}
				Directory.GetDirectories(CacheFullPath)
					.Where(path => !Children.Any(vm => vm.CacheFullPath == path))
					.ToList()
					.ForEach(path => Directory.Delete(path, recursive: true));
			}
		}

		private async Task SelectAsync()
		{
			BeforeSelect?.Invoke(this, null);

			await Task.CompletedTask;

			Selected?.Invoke(this, null);
		}

		#endregion Private Methods
	}
}