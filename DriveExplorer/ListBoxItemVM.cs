using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DriveExplorer {
    public class ListBoxItemVM : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Selected;

        private bool isSelected;

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


        public ItemModel Model { get; private set; }

        public ListBoxItemVM(ItemModel model) {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public ListBoxItemVM(string fullpath) {
            Model = new ItemModel(fullpath);
        }
        private void Select() {
            
        }

    }

}
