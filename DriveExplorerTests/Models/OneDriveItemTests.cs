using Cyc.MicrosoftApi;

using DriveExplorer.Tests;

using Microsoft.Graph;
using Microsoft.Identity.Client;

using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace DriveExplorer.Models.Tests {
	[TestFixtureSource(typeof(TestSource))]
	public class OneDriveItemTests {
		private DriveItem root;

		private readonly MicrosoftManager microsoftManager;
		private readonly IAccount account;

		public OneDriveItemTests(object[] param) {
			microsoftManager = (MicrosoftManager)param[0];
			account = (IAccount)param[1];
		}
		[OneTimeSetUp]
		public async Task OneTimeSetupAsync() {
			root = await microsoftManager.GetDriveRootAsync(account).ConfigureAwait(false);
		}

		[Test()]
		public void OneDriveItemTest() {
			var item = new OneDriveItem(microsoftManager, root, account);
			Assert.NotNull(item);
		}

		[Test()]
		public async Task GetChildrenAsyncTestAsync() {
			var item = new OneDriveItem(microsoftManager, root, account);
			await foreach (var child in item.GetChildrenAsync().ConfigureAwait(false)) {
				Console.WriteLine(child.Name);
				Assert.NotNull(child);
			}
		}
	}
}