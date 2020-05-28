using Cyc.MicrosoftApi;
using DriveExplorer.MicrosoftApi;
using DriveExplorer.MicrosoftApi.Tests;
using DriveExplorer.Models;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using NUnit.Framework;

using DriveExplorer.Tests;
using System;
using System.Threading.Tasks;

namespace DriveExplorer.ViewModels.Tests {
	[TestFixtureSource(typeof(TestSource))]
	public class ItemVMTests {
		private readonly MicrosoftManager microsoftManager;
		private readonly IAccount account;
		private DriveItem root;
		private ItemVM localItem;
		private ItemVM onedriveItem;

		public ItemVMTests(object[] param) {
			microsoftManager = (MicrosoftManager)param[0];
			account = (IAccount)param[1];
		}
		[SetUp]
		public void Setup() {
			localItem = new ItemVM(new LocalItem(@"C:\\"));
			onedriveItem = new ItemVM(new OneDriveItem(microsoftManager, root, account));
		}
		[OneTimeSetUp]
		public async Task OneTimeSetupAsync() {
			root = await microsoftManager.GetDriveRootAsync(account).ConfigureAwait(false);
		}
		[Test()]
		public void LocalItemVMTest() {
			Assert.NotNull(localItem);
		}

		[Test()]
		public void OneDriveItemVMTest() {
			Assert.NotNull(onedriveItem);
		}

		[Test()]
		public async Task LocalItemSetIsExpandedAsyncTestAsync() {
			await localItem.SetIsExpandedAsync(true).ConfigureAwait(false);
			Assert.True(localItem.IsExpanded);
			Assert.NotNull(localItem.Children[0]);
		}

		[Test()]
		public async Task OneDriveItemSetIsExpandedAsyncTestAsync() {
			await onedriveItem.SetIsExpandedAsync(true).ConfigureAwait(false);
			Assert.True(onedriveItem.IsExpanded);
			Assert.NotNull(onedriveItem.Children[0]);
		}

		[Test()]
		public async Task LocalItemSetIsSelectedAsyncTestAsync() {
			await localItem.SetIsSelectedAsync(true).ConfigureAwait(false);
			Assert.True(localItem.IsSelected);
		}

		[Test()]
		public async Task OneDriveItemSetIsSelectedAsyncTestAsync() {
			await onedriveItem.SetIsSelectedAsync(true).ConfigureAwait(false);
			Assert.True(onedriveItem.IsSelected);
		}

	}
}