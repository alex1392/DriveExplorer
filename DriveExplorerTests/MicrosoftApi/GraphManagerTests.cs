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
		private readonly GraphManager graphManager;
		private readonly IAccount account;

		public GraphManagerTests(object[] param) {
			graphManager = (GraphManager)param[0];
			account = (IAccount)param[1];
		}

		[Test()]
		public void GraphManagerTest() {
			Assert.NotNull(graphManager);
		}

		[Test()]
		public async Task GetMeTestAsync() {
			var u = await graphManager.GetUserAsync(account).ConfigureAwait(false);
			Assert.NotNull(u);
		}

		[Test()]
		public async Task GetDriveRootTestAsync() {
			var root = await graphManager.GetDriveRootAsync(account).ConfigureAwait(false);
			Assert.NotNull(root);
		}

		[Test()]
		public async Task GetChildrenTestAsync() {
			var root = await graphManager.GetDriveRootAsync(account).ConfigureAwait(false);
			var asyncEnumerable = graphManager.GetChildrenAsync(root.Id, account);
			await foreach (var child in asyncEnumerable) {
				Console.WriteLine(child.Name);
			}
			Assert.NotNull(asyncEnumerable);
		}
	}
}