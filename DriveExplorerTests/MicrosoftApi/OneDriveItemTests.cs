using Microsoft.Graph;
using Microsoft.Identity.Client;
using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace DriveExplorer.MicrosoftApi.Tests {
	[TestFixtureSource(typeof(MicrosoftApiSource))]
	public class OneDriveItemTests {
		private DriveItem root;

		public GraphManager GraphManager { get; }
		public IAccount Account { get; }

		public OneDriveItemTests(object[] param) {
			GraphManager = (GraphManager)param[0];
			Account = (IAccount)param[1];
		}
		[OneTimeSetUp]
		public async Task OneTimeSetupAsync() {
			root = await GraphManager.GetDriveRootAsync(Account).ConfigureAwait(false);
		}

		[Test()]
		public void OneDriveItemTest() {
			var item = new OneDriveItem(GraphManager, root, Account);
			Assert.NotNull(item);
		}

		[Test()]
		public async Task GetChildrenAsyncTestAsync() {
			var item = new OneDriveItem(GraphManager, root, Account);
			await foreach (var child in item.GetChildrenAsync().ConfigureAwait(false)) {
				Console.WriteLine(child.Name);
				Assert.NotNull(child);
			}
		}
	}
}