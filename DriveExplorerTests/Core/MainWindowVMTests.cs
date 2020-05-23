using Xunit;
using DriveExplorer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using DriveExplorer.IoC;
using Microsoft.Graph;
using DriveExplorer.MicrosoftApi;

namespace DriveExplorer.Core {
    public class MainWindowVMTestFixture {
        public MainWindowVM mainWindowVM;

        public MainWindowVMTestFixture() {
            IocContainer.Default.Register<IAuthenticationProvider>(() => AuthProvider.Default);
            IocContainer.Default.Register<AuthProvider>(() => AuthProvider.Default);
            IocContainer.Default.Register<GraphManager>();
            IocContainer.Default.Register<MainWindowVM>();
            IocContainer.Default.Register<LocalItemFactory>();
            IocContainer.Default.Register<OneDriveItemFactory>();
            IocContainer.Default.Register<OneDriveItem>();
            mainWindowVM = IocContainer.Default.GetSingleton<MainWindowVM>();
        }
    }
    public class MainWindowVMTests : IClassFixture<MainWindowVMTestFixture> {
        private readonly MainWindowVM mainWindowVM;

        public MainWindowVMTests(MainWindowVMTestFixture fixture) {
            mainWindowVM = fixture.mainWindowVM;
            foreach (var item in mainWindowVM.ItemVMs) {
                item.Selected += async (sender, e) => await mainWindowVM.TreeViewItem_SelectedAsync(sender);
            }
            foreach (var item in mainWindowVM.listBoxItemVMs) {
                item.Selected += async (sender, e) => await mainWindowVM.ListBoxItem_SelectedAsync(sender);
            }
        }

        [Fact()]
        public void TreeViewItem_SelectedTest() {
            Debug.WriteLine($"listBoxItems: {mainWindowVM.listBoxItemVMs.Count}"); // should be only one item (C:\)
            mainWindowVM.ItemVMs[0].IsSelected = true;
            Assert.True(mainWindowVM.listBoxItemVMs.Count > 1);
        }

        [Fact()]
        public void ListBoxItem_SelectedTest() {
            Debug.WriteLine($"IsExpanded: {mainWindowVM.ItemVMs[0].IsExpanded}"); // should be false
            mainWindowVM.listBoxItemVMs[0].IsSelected = true;
            Assert.True(mainWindowVM.ItemVMs[0].IsExpanded);
        }
    }
}