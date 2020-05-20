using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace TreeViewTest {
    public class TreeViewItemViewModel : INotifyPropertyChanged {
        private bool isExpanded;
        private bool isSelected;
        public ItemModel Model { get; private set; }

        public ObservableCollection<TreeViewItemViewModel> Items { get; set; } = new ObservableCollection<TreeViewItemViewModel>
        {
            null // add dummyItem for the expansion indicator
        };

        public TreeViewItemViewModel(ItemModel.Types type, string fullpath, bool isRoot = false) {
            Model = new ItemModel(type, fullpath, isRoot);
        }

        public TreeViewItemViewModel(ItemModel model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public event EventHandler Expanded;
        public event EventHandler Selected;
        public event PropertyChangedEventHandler PropertyChanged;

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
                if (value != isSelected) {
                    isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }
        private bool HaveExpanded =>
            Items.Count != 1 || Items[0] != null;

        private void Expand() {
            if (HaveExpanded)
                return;
            AttachChildren();
        }
        private void Select() {
            if (HaveExpanded) {
                return;
            }
            AttachChildren();
        }

        private void AttachChildren() {
            Items.Clear(); // clear the dummy item
            string[] subfolderPaths;
            try {
                subfolderPaths = Directory.GetDirectories(Model.FullPath);
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show(ex.Message);
                return;
            }
            foreach (var subfolderPath in subfolderPaths) {
                var subfolderViewModel = new TreeViewItemViewModel
                (
                    type: ItemModel.Types.Folder,
                    fullpath: subfolderPath
                );
                // listen to its subfolders' events, propagate event to root for user to handle. Note that the original sender is passed as sender in the current invocation
                subfolderViewModel.Expanded += (sender, e) =>
                    Expanded?.Invoke(sender, e);
                subfolderViewModel.Selected += (sender, e) =>
                    Selected?.Invoke(sender, e);
                Items.Add(subfolderViewModel);
            }
        }

        public override string ToString() {
            return $"{Model.Name}, Items: {Items.Count}";
        }
    }
}
