using Xunit;
using DriveExplorer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DriveExplorer.Tests {
    public class TreeViewItemVMTests {

        [Fact()]
        public void Expand_ItemsFilled() {
            var vm = new TreeViewItemVM(@"C:\");
            Debug.WriteLine($"Items: {vm.Items.Count}"); // should be one dummy item
            vm.IsExpanded = true;
            Assert.True(vm.Items.Count > 1);
        }
    }
}