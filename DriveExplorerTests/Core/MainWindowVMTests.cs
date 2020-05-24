using Xunit;
using DriveExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DriveExplorer.MicrosoftApi;
using DriveExplorer.IoC;
using System.Diagnostics;

namespace DriveExplorer.Core {
	public class MainWindowVMTestFixture {
		public MainWindowVM mainWindowVM;

		public MainWindowVMTestFixture() {
			IocContainer.Default.Register<AuthProvider>(() => new AuthProvider(Urls.Auth.Organizations));
			IocContainer.Default.Register<GraphManager>();
			IocContainer.Default.Register<MainWindowVM>();
			IocContainer.Default.Register<LocalItemFactory>();
			IocContainer.Default.Register<OneDriveItemFactory>();
			IocContainer.Default.Register<OneDriveItem>();
			var authProvider = IocContainer.Default.GetSingleton<AuthProvider>();
			var token = authProvider.GetAccessTokenWithUsernamePassword().Result;
			mainWindowVM = IocContainer.Default.GetSingleton<MainWindowVM>();
		}
	}
	public class MainWindowVMTests : IClassFixture<MainWindowVMTestFixture>, IDisposable {
		private readonly MainWindowVM mainWindowVM;


		public MainWindowVMTests(MainWindowVMTestFixture fixture) {
			mainWindowVM = fixture.mainWindowVM;
		}

		private async Task SetupAsync() {
			mainWindowVM.GetLocalDrives();
			await mainWindowVM.LoginOneDrive();
			mainWindowVM.StartPage();
			AttachEvent();
		}

		/// <summary>
		/// Should be called everytime new items added to MainWindowVM
		/// </summary>
		private void AttachEvent() {
			foreach (var item in mainWindowVM.TreeItemVMs) {
				item.Selected += async (sender, e) =>
					await mainWindowVM.TreeItem_SelectedAsync(sender);
			}
			foreach (var item in mainWindowVM.CurrentItemVMs) {
				item.Selected += async (sender, e) =>
					await mainWindowVM.CurrentItem_SelectedAsync(sender);
			}
		}

		public void Dispose() {
			mainWindowVM.Reset();
		}

		[Fact()]
		public async Task TreeViewItem_SelectedTestAsync() {
			await SetupAsync();
			Debug.WriteLine($"listBoxItems: {mainWindowVM.CurrentItemVMs.Count}"); // should be only one item (C:\)
			await mainWindowVM.TreeItemVMs[0].SetIsSelectedAsync(true);
			Assert.True(mainWindowVM.CurrentItemVMs.Count > 1);
		}

		[Fact()]
		public async Task ListBoxItem_SelectedTestAsync() {
			await SetupAsync();
			await mainWindowVM.CurrentItemVMs[0].SetIsSelectedAsync(true);
			Assert.True(mainWindowVM.TreeItemVMs[0].IsExpanded);
		}

		[Fact]
		public async Task OneDriveTreeItem_SelectedTestAsync() {
			await SetupAsync();
			await mainWindowVM.TreeItemVMs[1].SetIsSelectedAsync(true);
			Assert.True(mainWindowVM.TreeItemVMs[1].IsExpanded);
		}

		[Fact()]
		public void ResetTest() {
			throw new NotImplementedException();
		}

		[Fact()]
		public void LoginOneDriveTest() {
			throw new NotImplementedException();
		}

		[Fact()]
		public void GetLocalDrivesTest() {
			throw new NotImplementedException();
		}

		[Fact()]
		public void GetOneDriveAsyncTest() {
			throw new NotImplementedException();
		}

		[Fact()]
		public void StartPageTest() {
			throw new NotImplementedException();
		}

		[Fact()]
		public void TreeItem_SelectedAsyncTest() {
			throw new NotImplementedException();
		}

		[Fact()]
		public void CurrentItem_SelectedAsyncTest() {
			throw new NotImplementedException();
		}
	}
}