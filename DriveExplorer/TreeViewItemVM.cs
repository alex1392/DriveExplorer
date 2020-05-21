using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DriveExplorer {
    public class TreeViewItemVM : INotifyPropertyChanged {
        private bool isExpanded;
        private bool isSelected;

        public static TreeViewItemVM Empty { get; } = new TreeViewItemVM();
        public ItemModel Model { get; private set; }

        public bool IsExpanded {
            get => isExpanded;
            set {
                if (value == true) {
                    Expand();
                    Expanded?.Invoke(this, null);
                }
                if (value != isExpanded) {
                    isExpanded = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isExpanded)));
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

        public ObservableCollection<TreeViewItemVM> Items { get; set; } = new ObservableCollection<TreeViewItemVM>
        {
            null // add dummyItem for the expansion indicator
        };

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Expanded;
        public event EventHandler Selected;

        public TreeViewItemVM(string fullpath) {
            Model = new ItemModel(fullpath);
        }

        public TreeViewItemVM(ItemModel model) {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public override string ToString() {
            return $"{Model.Name}, Items: {Items.Count}";
        }

        private TreeViewItemVM() {
        }

        private bool HaveExpanded =>
            Items.Count != 1 || Items[0] != null;

        /// <summary>
        /// Add subfolders to <see cref="Items"/>, returns <see cref="true"/> if succeeded.
        /// </summary>
        /// <returns><see cref="bool"/> indicating whether this operation is successful.</returns>
        private bool Expand() {
            if (HaveExpanded) {
                return false;
            }
            Items.Clear(); // clear dummy item
            try {
                var subfolderPaths = Directory.GetDirectories(Model.FullPath);
                foreach (var subfolderPath in subfolderPaths) {
                    Items.Add(new TreeViewItemVM(subfolderPath));
                }
                return true;
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private void Select() {

        }

    }
}
