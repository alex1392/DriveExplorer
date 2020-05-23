using DriveExplorer.IoC;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DriveExplorer {
    public class ItemVM : INotifyPropertyChanged {
        private bool _isExpanded;
        private bool haveExpanded = false;
        private bool _isSelected;
		/// <summary>
		/// Invoke <see cref="PropertyChanged"/> event
		/// </summary>
		/// <value></value>
        private bool isSelected {
            get => _isSelected;
            set {
                if (value != _isSelected) {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        /// <summary>
        /// Invoke <see cref="PropertyChanged"/> event
        /// </summary>
        private bool isExpanded {
            get => _isExpanded;
            set {
                if (value != _isExpanded) {
                    _isExpanded = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
                }
            }
        }

        public static ItemVM Empty { get; } = new ItemVM();
        public IItem Item { get; private set; }
        /// <summary>
        /// Change the state of expansion and invoke <see cref="ExpandAsync"/> without await. To expand item programmically, directly call <see cref="ExpandAsync"/> instead.
        /// </summary>
        public bool IsExpanded {
            get => isExpanded;
            set {
                if (value != isExpanded) {
                    isExpanded = value;
                }
                if (value == true) {
                    ExpandAsync();
                }
            }
        }
        /// <summary>
        /// Change the state of selection and invoke <see cref="SelectAsync"/> without await. To select item programmically, directly call <see cref="SelectAsync"/> instead.
        /// </summary>
        public bool IsSelected {
            get => isSelected;
            set {
                if (value != IsSelected) {
                    isSelected = value;
                }
                if (value == true) {
                    SelectAsync();
                }
            }
        }

        public ObservableCollection<ItemVM> Children { get; set; } = new ObservableCollection<ItemVM>
        {
            null // add dummyItem for the expansion indicator
        };

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Expanded;
        public event EventHandler Selected;

        private ItemVM() {
        }

        public ItemVM(IItem model) : this() {
            Item = model ?? throw new ArgumentNullException(nameof(model));
        }

        public override string ToString() {
            return $"{Item.Name}, Items: {Children.Count}";
        }

        public async Task ExpandAsync() {
            if (!Item.Type.Is(ItemTypes.Folders)) {
                return;
            }
            if (isExpanded != true) {
                isExpanded = true; // invoke propertychanged 
            }
            if (haveExpanded) {
                return;
            }
            haveExpanded = true;
            Children.Clear(); // clear dummy item
            await foreach (var item in Item.GetChildrenAsync()) {
                Children.Add(new ItemVM(item));
            }
            Expanded?.Invoke(this, null); // invoke event
        }

        public async Task SelectAsync() {
            if (isSelected != true) {
                isSelected = true; // invoke propertychanged
            }
			

			Selected?.Invoke(this, null); // invoke event
        }

    }
}
