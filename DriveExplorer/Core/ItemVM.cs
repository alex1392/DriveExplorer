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
        private bool isSelected;
        /// <summary>
        /// Invoke <see cref="PropertyChanged"/> event
        /// </summary>
        private bool isExpanded {
            get => _isExpanded;
            set {
                if (value != _isExpanded) {
                    _isExpanded = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isExpanded)));
                }
            }
        }

        public static ItemVM Empty { get; } = new ItemVM();
        public IItem Item { get; private set; }
        /// <summary>
        /// Invoke <see cref="ExpandAsync"/> event.
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

        public bool IsSelected {
            get => isSelected;
            set {
                if (value == true) {
                    Select();
                    Selected?.Invoke(this, null);
                }
                if (value != IsSelected) {
                    isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
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
            Expanded?.Invoke(this, null);
        }

        private void Select() {

        }

    }
}
