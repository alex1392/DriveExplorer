using Microsoft.Graph;

using NUnit.Framework;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DriveExplorer.MicrosoftApi.Tests {

	[TestFixtureSource(typeof(MicrosoftApiSource))]
	public class GraphManagerTests {
		private GraphManager graphManager;

		public GraphManagerTests(AuthProvider _, GraphManager graphManager) {
			this.graphManager = graphManager;
		}

		[Test()]
		public void GraphManagerTest() {
			Assert.NotNull(graphManager);
		}

		[Test()]
		public async Task GetMeTestAsync() {
			var user = await graphManager.GetCurrentUserAsync().ConfigureAwait(false);
			Assert.NotNull(user);
		}

		[Test()]
		public async Task GetDriveRootTestAsync() {
			var root = await graphManager.GetDriveRootAsync().ConfigureAwait(false);
			Assert.NotNull(root);
		}

		[Test()]
		public async Task SearchDriveTestAsync() {
			var file = (await graphManager.SearchDriveAsync("LICENSE.txt",
				new[] {
					new QueryOption("$top", "5"),
					new QueryOption("$select", GraphManager.Selects.name + "," + GraphManager.Selects.id)
					}).ConfigureAwait(false)).First();
			Assert.NotNull(file);
		}

		[Test()]
		public async Task GetContentTestAsync() {
			var item = (await graphManager.SearchDriveAsync("LICENSE.txt")).First();
			var file = await graphManager.GetContentAsync(item.Id).ConfigureAwait(false);
			Assert.NotNull(file);
		}

		[Test()]
		public async Task GetChildrenTestAsync() {
			var root = await graphManager.GetDriveRootAsync().ConfigureAwait(false);
			var asyncEnumerable = graphManager.GetChildrenAsync(root.Id);
			await foreach (var child in asyncEnumerable) {
				Console.WriteLine(child.Name);
			}
			Assert.NotNull(asyncEnumerable);
		}

		[Test()]
		public async Task UploadFileTestAsync() {
			var content = "aaa";
			var parentId = (await graphManager.GetDriveRootAsync().ConfigureAwait(false)).Id;
			var filename = "aaa.txt";
			//When
			var response = await graphManager.UploadFileAsync(parentId, filename, content);
			Console.WriteLine(response);
			//Then
			Assert.NotNull(response);
		}

		[Test()]
		public async Task UpdateFileTestAsync() {
			var itemId = (await graphManager.SearchDriveAsync("LICENSE.txt").ConfigureAwait(false)).First().Id;
			var content = "aaa";
			//When
			var driveItem = await graphManager.UpdateFileAsync(itemId, content).ConfigureAwait(false);
			var stream = await graphManager.GetContentAsync(driveItem.Id).ConfigureAwait(false);
			using var reader = new StreamReader(stream);
			Console.WriteLine(reader.ReadToEnd());
			//Then
			Assert.NotNull(driveItem);
		}
	}
}