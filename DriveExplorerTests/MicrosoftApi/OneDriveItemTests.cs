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
		public User User { get; }

		public OneDriveItemTests(GraphManager graphManager, IAccount account, User user) {
			GraphManager = graphManager;
			Account = account;
			User = user;
		}
		[OneTimeSetUp]
		public async Task OneTimeSetupAsync() {
			root = await GraphManager.GetDriveRootAsync(User.Id).ConfigureAwait(false);
		}

		[Test()]
		public void OneDriveItemTest() {
			var item = new OneDriveItem(GraphManager, root, User, Account);
			Assert.NotNull(item);
		}

		[Test()]
		public async Task GetChildrenAsyncTestAsync() {
			var item = new OneDriveItem(GraphManager, root, User, Account);
			await foreach (var child in item.GetChildrenAsync().ConfigureAwait(false)) {
				Console.WriteLine(child.Name);
				Assert.NotNull(child);
			}
		}
	}
}