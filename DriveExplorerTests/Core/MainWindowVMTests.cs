using Xunit;
using DriveExplorer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DriveExplorer.Core {
    public class MainWindowVMTests {
        private MainWindowVM mainWindowVM;

        public MainWindowVMTests() {
            mainWindowVM = new MainWindowVM();
            foreach (var item in mainWindowVM.treeViewItemVMs) {
                item.Selected += mainWindowVM.TreeViewItem_Selected;
            }
            foreach (var item in mainWindowVM.listBoxItemVMs) {
                item.Selected += mainWindowVM.ListBoxItem_Selected;
            }
        }

        [Fact()]
        public void TreeViewItem_SelectedTest() {
            Debug.WriteLine($"listBoxItems: {mainWindowVM.listBoxItemVMs.Count}"); // should be only one item (C:\)
            mainWindowVM.treeViewItemVMs[0].IsSelected = true;
            Assert.True(mainWindowVM.listBoxItemVMs.Count > 1);
        }

        [Fact()]
        public void ListBoxItem_SelectedTest() {
            Debug.WriteLine($"IsExpanded: {mainWindowVM.treeViewItemVMs[0].IsExpanded}"); // should be false
            mainWindowVM.listBoxItemVMs[0].IsSelected = true;
            Assert.True(mainWindowVM.treeViewItemVMs[0].IsExpanded);
        }
    }
}