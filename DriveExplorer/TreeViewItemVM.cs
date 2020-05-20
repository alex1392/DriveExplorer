using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DriveExplorer {
    public class TreeViewItemVM : INotifyPropertyChanged {
        public static readonly TreeViewItemVM Empty = new TreeViewItemVM();
        private bool isExpanded;
        private bool isSelected;

        public ItemModel Model { get; private set; }
        /// <summary>
        /// Twoway bound to <see cref="TreeViewItem"/>, to change its state from ViewModel.
        /// </summary>
        public bool IsExpanded {
            get => isExpanded;
            set {
                if (value != isExpanded) {
                    isExpanded = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isExpanded)));
                    System.Diagnostics.Debug.WriteLine("expanded");
                }
            }
        }
        /// <summary>
        /// Twoway bound to <see cref="TreeViewItem"/>, to change its state from ViewModel.
        /// </summary>
        public bool IsSelected {
            get => isSelected; 
            set {
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

        public TreeViewItemVM(ItemModel.Types type, string fullpath, bool isRoot = false) {
            Model = new ItemModel(type, fullpath, isRoot);
        }

        public TreeViewItemVM(ItemModel model) {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        private TreeViewItemVM() {
        }

        private bool HaveExpanded =>
            Items.Count > 1 || Items[0] != null;


        /// <summary>
        /// Add subfolders to <see cref="Items"/>, returns <see cref="true"/> if succeeded.
        /// </summary>
        /// <returns><see cref="bool"/> indicating whether this operation is successful.</returns>
        public bool Expand() {
            if (HaveExpanded) {
                return false;
            }
            Items.Clear(); // clear dummy item
            try {
                var subfolderPaths = Directory.GetDirectories(Model.FullPath);
                foreach (var subfolderPath in subfolderPaths) {
                    Items.Add(new TreeViewItemVM(ItemModel.Types.Folder, subfolderPath));
                }
                return true;
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public override string ToString() {
            return $"{Model.Name}, Items: {Items.Count}";
        }
    }
}
