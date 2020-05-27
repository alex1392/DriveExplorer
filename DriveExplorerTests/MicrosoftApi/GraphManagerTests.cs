using Microsoft.Graph;
using Microsoft.Identity.Client;
using NUnit.Framework;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DriveExplorer.MicrosoftApi.Tests {

	[TestFixtureSource(typeof(MicrosoftApiSource))]
	public class GraphManagerTests {
		private GraphManager graphManager;
		private readonly IAccount account;
		private readonly User user;

		public GraphManagerTests(GraphManager graphManager, IAccount account, User user) {
			this.graphManager = graphManager;
			this.account = account;
			this.user = user;
		}

		[Test()]
		public void GraphManagerTest() {
			Assert.NotNull(graphManager);
		}

		[Test()]
		public async Task GetMeTestAsync() {
			var u = await graphManager.GetUserAsync(user.Id).ConfigureAwait(false);
			Assert.NotNull(u);
		}

		[Test()]
		public async Task GetDriveRootTestAsync() {
			var root = await graphManager.GetDriveRootAsync(user.Id).ConfigureAwait(false);
			Assert.NotNull(root);
		}

		[Test()]
		public async Task GetChildrenTestAsync() {
			var root = await graphManager.GetDriveRootAsync(user.Id).ConfigureAwait(false);
			var asyncEnumerable = graphManager.GetChildrenAsync(root.Id, user.Id);
			await foreach (var child in asyncEnumerable) {
				Console.WriteLine(child.Name);
			}
			Assert.NotNull(asyncEnumerable);
		}
	}
}