using Microsoft.Graph;
using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace DriveExplorer.MicrosoftApi.Tests {
	[TestFixtureSource(typeof(MicrosoftApiSource))]
	public class OneDriveItemTests {
		private User user;
		private DriveItem root;

		public AuthProvider AuthProvider { get; }
		public GraphManager GraphManager { get; }

		public OneDriveItemTests(AuthProvider authProvider, GraphManager graphManager) {
			AuthProvider = authProvider;
			GraphManager = graphManager;
		}
		[OneTimeSetUp]
		public async Task OneTimeSetupAsync() {
			user = await GraphManager.GetCurrentUserAsync().ConfigureAwait(false);
			root = await GraphManager.GetDriveRootAsync().ConfigureAwait(false);
		}

		[Test()]
		public void OneDriveItemTest() {
			var item = new OneDriveItem(GraphManager, root, user, AuthProvider.CurrentUserAccount);
			Assert.NotNull(item);
		}

		[Test()]
		public async Task GetChildrenAsyncTestAsync() {
			var item = new OneDriveItem(GraphManager, root, user, AuthProvider.CurrentUserAccount);
			await foreach (var child in item.GetChildrenAsync().ConfigureAwait(false)) {
				Console.WriteLine(child.Name);
				Assert.NotNull(child);
			}
		}
	}
}