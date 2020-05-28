using Cyc.GoogleApi;
using DriveExplorer.Tests;

using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace DriveExplorer.GoogleApi.Tests {
	[TestFixtureSource(typeof(TestSource))]
	public class GoogleManagerTests {
		private GoogleManager googleManager;

		public GoogleManagerTests(object[] param) {
			googleManager = (GoogleManager)param[2];
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