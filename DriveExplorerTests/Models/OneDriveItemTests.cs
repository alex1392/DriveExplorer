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

		public MicrosoftManager microsoftManager { get; }
		public IAccount Account { get; }

		public OneDriveItemTests(object[] param) {
			microsoftManager = (MicrosoftManager)param[0];
			Account = (IAccount)param[1];
		}
		[OneTimeSetUp]
		public async Task OneTimeSetupAsync() {
			root = await microsoftManager.GetDriveRootAsync(Account).ConfigureAwait(false);
		}

		[Test()]
		public void OneDriveItemTest() {
			var item = new OneDriveItem(microsoftManager, root, Account);
			Assert.NotNull(item);
		}

		[Test()]
		public async Task GetChildrenAsyncTestAsync() {
			var item = new OneDriveItem(microsoftManager, root, Account);
			await foreach (var child in item.GetChildrenAsync().ConfigureAwait(false)) {
				Console.WriteLine(child.Name);
				Assert.NotNull(child);
			}
		}
	}
}