using Cyc.MicrosoftApi;
using DriveExplorer.Tests;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using NUnit.Framework;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DriveExplorer.MicrosoftApi.Tests {

	[TestFixtureSource(typeof(TestSource))]
	public class MicrosoftManagerTests {
		private readonly MicrosoftManager microsoftManager;
		private readonly IAccount account;

		public MicrosoftManagerTests(object[] param) {
			microsoftManager = (MicrosoftManager)param[0];
			account = (IAccount)param[1];
		}

		[Test()]
		public void microsoftManagerTest() {
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
			var asyncEnumerable = microsoftManager.GetChildrenAsync(root.Id, account);
			await foreach (var child in asyncEnumerable) {
				Console.WriteLine(child.Name);
			}
			Assert.NotNull(asyncEnumerable);
		}
	}
}