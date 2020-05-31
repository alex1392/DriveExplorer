using Cyc.MicrosoftApi;

using DriveExplorer.Tests;

using Microsoft.Identity.Client;

using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace DriveExplorer.MicrosoftApi.Tests {

	[TestFixtureSource(typeof(TestSource))]
	public class MicrosoftManagerTests {
		private readonly MicrosoftApiManager microsoftManager;
		private readonly IAccount account;

		public MicrosoftManagerTests(object[] param) {
			microsoftManager = (MicrosoftApiManager)param[0];
			account = (IAccount)param[1];
		}

		[Test()]
		public void MicrosoftManagerTest() {
			Assert.NotNull(microsoftManager);
		}

		[Test()]
		public async Task GetMeTestAsync() {
			var u = await microsoftManager.GetUserAsync(account).ConfigureAwait(false);
			Assert.NotNull(u);
		}

		[Test()]
		public async Task GetDriveRootTestAsync() {
			var root = await microsoftManager.GetDriveRootAsync(account).ConfigureAwait(false);
			Assert.NotNull(root);
		}

		[Test()]
		public async Task GetChildrenTestAsync() {
			var root = await microsoftManager.GetDriveRootAsync(account).ConfigureAwait(false);
			await foreach (var child in microsoftManager.GetChildrenAsync(account, root.Id).ConfigureAwait(false)) {
				Console.WriteLine(child.Name);
				Assert.NotNull(child);
			}
		}
	}
}