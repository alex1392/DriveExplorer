using Xunit;
using DriveExplorer;
using System;
using System.Collections.Generic;
using System.Text;
using DriveExplorer.IoC;

namespace DriveExplorer.Core {
    public class ItemTests {
        public ItemTests() {
            IocContainer.Default.Register<LocalItemFactory>();
        }
        [Theory]
        [InlineData(@"C:\", ItemTypes.LocaDrive)]
        [InlineData(@"C:\path\doc.png", ItemTypes.IMG)]
        [InlineData(@"C:\path\MainWindow.xaml", ItemTypes.File)]
        [InlineData(@"C:\path\", ItemTypes.Folder)]
        public void ItemModelTypeTest(string fullPath, ItemTypes expected) {
            var item = IocContainer.Default.GetSingleton<LocalItemFactory>().Create(fullPath);
            var actual = item.Type;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(@"C:\", "C:")]
        [InlineData(@"C:\path\doc.png", "doc.png")]
        [InlineData(@"C:\path\MainWindow.xaml", "MainWindow.xaml")]
        [InlineData(@"C:\path\", "path")]
        public void ItemModelNameTest(string fullPath, string expected) {
            var item = IocContainer.Default.GetSingleton<LocalItemFactory>().Create(fullPath);
            var actual = item.Name;
            Assert.Equal(expected, actual);
        }


    }
}