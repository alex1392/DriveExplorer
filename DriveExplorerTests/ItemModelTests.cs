using Xunit;
using DriveExplorer;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveExplorer.Tests {
    public class ItemModelTests {
        [Theory]
        [InlineData(@"C:\", ItemModel.Types.Drive)]
        [InlineData(@"C:\path\doc.png", ItemModel.Types.IMG)]
        [InlineData(@"C:\path\MainWindow.xaml", ItemModel.Types.File)]
        [InlineData(@"C:\path\", ItemModel.Types.Folder)]
        public void ItemModelTypeTest(string fullPath, ItemModel.Types expected) {
            var item = new ItemModel(fullPath);
            var actual = item.Type;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(@"C:\", "C:")]
        [InlineData(@"C:\path\doc.png", "doc.png")]
        [InlineData(@"C:\path\MainWindow.xaml", "MainWindow.xaml")]
        [InlineData(@"C:\path\", "path")]
        public void ItemModelNameTest(string fullPath, string expected) {
            var item = new ItemModel(fullPath);
            var actual = item.Name;
            Assert.Equal(expected, actual);
        }


    }
}