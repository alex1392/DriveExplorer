using Cyc.GoogleApi;
using DriveExplorer.MicrosoftApi.Tests;
using DriveExplorer.Tests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveExplorer.GoogleApi.Tests {
	[TestFixtureSource(typeof(TestSource))]
	public class GoogleManagerTests {
		private GoogleManager googleManager;

		public GoogleManagerTests(object[] param) {
			googleManager = (GoogleManager)param[3];
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