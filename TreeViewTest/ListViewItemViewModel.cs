using System;
using System.ComponentModel;

namespace TreeViewTest {
    public class ListViewItemViewModel : INotifyPropertyChanged {
        private bool isSelected;

        public ItemModel Model { get; private set; }
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

        private void Select() {
            
        }

        public ListViewItemViewModel(ItemModel model) {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public ListViewItemViewModel(ItemModel.Types type, string fullpath, bool isRoot = false) {
            Model = new ItemModel(type, fullpath, isRoot);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Selected;
    }

}
