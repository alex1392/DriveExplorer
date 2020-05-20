using System;
using System.ComponentModel;

namespace DriveExplorer {
    public class ListBoxItemVM {

        public ItemModel Model { get; private set; }

        public ListBoxItemVM(ItemModel model) {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public ListBoxItemVM(ItemModel.Types type, string fullpath, bool isRoot = false) {
            Model = new ItemModel(type, fullpath, isRoot);
        }
    }

}
