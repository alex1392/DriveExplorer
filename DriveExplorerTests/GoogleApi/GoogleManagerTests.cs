using NUnit.Framework;
using DriveExplorer.GoogleApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveExplorer.GoogleApi.Tests {
	[TestFixture()]
	public class GoogleManagerTests {
		private GoogleManager googleManager;

		public GoogleManagerTests() {
			googleManager = new GoogleManager();
		}

		[Test()]
		public async Task GetDriveRootTestAsync() {
			var root = await googleManager.GetDriveRootAsync().ConfigureAwait(false);
			Assert.NotNull(root);
		}

		[Test()]
		public async Task GetChildrenTestAsync() {
			var root = await googleManager.GetDriveRootAsync().ConfigureAwait(false);
			await foreach (var child in googleManager.GetChildrenAsync(root.Id).ConfigureAwait(false)) {
				Console.WriteLine(child.Name);
				Assert.NotNull(child);
			}
		}
	}
}