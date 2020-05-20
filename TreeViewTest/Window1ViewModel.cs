using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Documents;

namespace TreeViewTest {
    public class Window1ViewModel : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        public Window1ViewModel() {
            var drives = Directory.GetLogicalDrives();
            foreach (var drive in drives) {
                var treeItem = new TreeViewItemViewModel
                    (
                        type: ItemModel.Types.Drive,
                        fullpath: drive,
                        isRoot: true
                    );
                treeItem.Expanded += TreeViewItem_Expanded;
                treeItem.Selected += TreeViewItem_Selected;
                TreeViewItems.Add(treeItem);

                var listItem = new ListViewItemViewModel
                    (
                        type: ItemModel.Types.Drive,
                        fullpath: drive,
                        isRoot: true
                    );
                listItem.Selected += ListViewItem_Selected;
                ListViewItems.Add(listItem);
            }
        }

        public void TreeViewItem_Selected(object sender, EventArgs e) {
            // sender must be the original sender of this event 
            if (!(sender is TreeViewItemViewModel treeViewModel)) {
                Console.WriteLine("Invalid sender");
                return;
            }
            // update list view according to the selected folder in treeview
            ListViewItems.Clear();
            foreach (var treeViewItem in treeViewModel.Items) {
                var listViewItem = new ListViewItemViewModel(treeViewItem.Model);
                listViewItem.Selected += ListViewItem_Selected;
                ListViewItems.Add(listViewItem);
            }
        }

        public void ListViewItem_Selected(object sender, EventArgs e) {
            if (!(sender is ListViewItemViewModel listViewModel)) {
                Console.WriteLine("Invalid sender");
                return;
            }
            // get every level of directories
            var directories = listViewModel.Model.FullPath.Split(Path.DirectorySeparatorChar)
                                                    .Where(s => !string.IsNullOrEmpty(s));
            // expand every level of directories
            var treeParent = TreeViewItems;
            TreeViewItemViewModel child = null;
            foreach (var dir in directories) {
                child = treeParent.First(t => t.Model.Name == dir);
                child.IsExpanded = true;
                treeParent = child.Items;
            }
            child.IsSelected = true; // select the deepest folder to update list view
        }

        public void TreeViewItem_Expanded(object sender, EventArgs e) {
            // sender must be the original sender of this event 

        }

        public ObservableCollection<TreeViewItemViewModel> TreeViewItems { get; private set; } = new ObservableCollection<TreeViewItemViewModel>();

        public ObservableCollection<ListViewItemViewModel> ListViewItems { get; private set; } = new ObservableCollection<ListViewItemViewModel>();
    }

}
