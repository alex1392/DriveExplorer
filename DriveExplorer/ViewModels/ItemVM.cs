using DriveExplorer.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace DriveExplorer.ViewModels {
	public class ItemVM : INotifyPropertyChanged, IEquatable<ItemVM> {

		private bool isExpanded;
		private bool haveExpanded = false;
		private bool isSelected;

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
		/// <summary>
		/// Optional property Injection
		/// </summary>
		public MainWindowVM MainWindowVM { get; set; }
		public IItem Item { get; private set; }

		public ObservableCollection<ItemVM> Children { get; set; } = new ObservableCollection<ItemVM>
		{
			null // add dummyItem for the expansion indicator
        };

		public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler Expanded;
		public event EventHandler Selected;

		public ItemVM(IItem model, MainWindowVM mainWindowVM = null) {
			Item = model;
			MainWindowVM = mainWindowVM;
		}

		/// <summary>
		/// Set <see cref="IsExpanded"/> asynchronizly.
		/// </summary>
		public async Task SetIsExpandedAsync(bool value) {
			if (value != isExpanded) {
				isExpanded = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
			}
			await ExpandAsync().ConfigureAwait(false);
		}
		/// <summary>
		/// Set <see cref="IsSelected"/> asynchronizly
		/// </summary>
		public async Task SetIsSelectedAsync(bool value) {
			if (value != isSelected) {
				isSelected = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
			}
			await SelectAsync().ConfigureAwait(false);
		}

		private async Task ExpandAsync() {
			if (!Item.Type.Is(ItemTypes.Folders)) {
				return;
			}
			// attach children
			if (haveExpanded) {
				return;
			}
			ShowSpinner();
			haveExpanded = true;
			Children.Clear(); // clear dummy item
			await foreach (var item in Item.GetChildrenAsync().ConfigureAwait(true)) {
				Children.Add(new ItemVM(item)
				{
					MainWindowVM = MainWindowVM
				});
			}
			Expanded?.Invoke(this, null); // invoke event
			HideSpinner();
		}

		private void ShowSpinner() {
			if (MainWindowVM != null) {
				MainWindowVM.SpinnerVisibility = Visibility.Visible;
			}
		}
		private void HideSpinner() {
			if (MainWindowVM != null) {
				MainWindowVM.SpinnerVisibility = Visibility.Collapsed;
			}
		}

		private async Task SelectAsync() {


			Selected?.Invoke(this, null); // invoke event
		}
		#region Overrides

		public override string ToString() {
			return $"{Item.Name}, Items: {Children.Count}";
		}

		public override bool Equals(object obj) {
			return Equals(obj as ItemVM);
		}

		public bool Equals(ItemVM other) {
			return other != null &&
				Item.Name == other.Item.Name &&
				Item.Type == other.Item.Type &&
				Item.FullPath == other.Item.FullPath;
		}

		public override int GetHashCode() {
			var hashCode = 424228742;
			hashCode = hashCode * -1521134295 + isExpanded.GetHashCode();
			hashCode = hashCode * -1521134295 + haveExpanded.GetHashCode();
			hashCode = hashCode * -1521134295 + isSelected.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<IItem>.Default.GetHashCode(Item);
			hashCode = hashCode * -1521134295 + EqualityComparer<ObservableCollection<ItemVM>>.Default.GetHashCode(Children);
			return hashCode;
		}

		public static bool operator ==(ItemVM left, ItemVM right) {
			return EqualityComparer<ItemVM>.Default.Equals(left, right);
		}

		public static bool operator !=(ItemVM left, ItemVM right) {
			return !(left == right);
		}
		#endregion
	}
}
