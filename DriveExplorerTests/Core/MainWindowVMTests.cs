using System.Linq;
using Xunit;
using DriveExplorer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using DriveExplorer.IoC;
using Microsoft.Graph;
using DriveExplorer.MicrosoftApi;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace DriveExplorer.Core {
	public class MainWindowVMTestFixture {
		public MainWindowVM mainWindowVM;

		public MainWindowVMTestFixture() {
			Func<AuthProvider> factory = () =>
				new AuthProvider(
					new ConfigurationBuilder()
					.AddUserSecrets<AuthProviderTests>()
					.Build()
					, Urls.Auth.Organizations);
			IocContainer.Default.Register<AuthProvider>(factory);
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
			await mainWindowVM.GetOneDriveAsync();
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

	}
}