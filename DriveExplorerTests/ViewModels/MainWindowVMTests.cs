using DriveExplorer.MicrosoftApi;
using DriveExplorer.MicrosoftApi.Tests;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace DriveExplorer.ViewModels.Tests {
	[TestFixtureSource(typeof(MicrosoftApiSource))]
	public class MainWindowVMTests {
		private readonly GraphManager graphManager;
		private readonly IAccount account;
		private readonly MainWindowVM mainWindowVM;

		public MainWindowVMTests(GraphManager graphManager, IAccount account) {
			this.graphManager = graphManager;
			this.account = account;
			mainWindowVM = new MainWindowVM(graphManager);
		}

		[TearDown]
		public void TearDown() {
			mainWindowVM.Reset();
		}

		[Test()]
		public void MainWindowVMTest() {
			Assert.NotNull(mainWindowVM);
		}

		[Test()]
		public void GetLocalDrivesTest() {
			mainWindowVM.GetLocalDrives();
			Assert.That(mainWindowVM.TreeItemVMs.Count > 0);
			Assert.That(mainWindowVM.CurrentItemVMs.Count > 0);
		}

		[Test()]
		public async Task AutoLoginOneDriveTestAsync() {
			await mainWindowVM.AutoLoginOneDrive().ConfigureAwait(false);
			Assert.That(mainWindowVM.TreeItemVMs.Count > 0);
			Assert.That(mainWindowVM.CurrentItemVMs.Count > 0);
		}

		[Test()]
		public async Task LogoutOneDriveAsyncTestAsync() {
			await mainWindowVM.AutoLoginOneDrive().ConfigureAwait(false);
			var originalCount = mainWindowVM.TreeItemVMs.Count;
			await mainWindowVM.LogoutOneDriveAsync().ConfigureAwait(false);
			var nowCount = mainWindowVM.TreeItemVMs.Count;
			Assert.That(nowCount < originalCount);
		}

		[Test()]
		public async Task TreeItem_SelectedAsyncTestAsync() {
			mainWindowVM.GetLocalDrives();
			var itemVM = mainWindowVM.TreeItemVMs[0];
			await mainWindowVM.TreeItem_SelectedAsync(itemVM).ConfigureAwait(false);
			Assert.That(mainWindowVM.TreeItemVMs[0].Children[0] != null);
		}

		[Test()]
		public async Task CurrentItem_SelectedAsyncTestAsync() {
			mainWindowVM.GetLocalDrives();
			var itemVM = mainWindowVM.CurrentItemVMs[0];
			await mainWindowVM.CurrentItem_SelectedAsync(itemVM).ConfigureAwait(false);
			Assert.That(mainWindowVM.TreeItemVMs[0].Children[0] != null);
		}

		[Test()]
		public void ResetTest() {
			mainWindowVM.GetLocalDrives();
			mainWindowVM.Reset();
			Assert.That(mainWindowVM.TreeItemVMs.Count == 0);
			Assert.That(mainWindowVM.CurrentItemVMs.Count == 0);
		}
	}
}