using Cyc.MicrosoftApi;

using DriveExplorer.Tests;

using Microsoft.Identity.Client;

using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace DriveExplorer.MicrosoftApi.Tests {

	[TestFixtureSource(typeof(TestSource))]
	public class MicrosoftManagerTests {

		#region Private Fields

		private readonly IAccount account;
		private readonly MicrosoftApiManager microsoftManager;

		#endregion Private Fields

		#region Public Constructors

		public MicrosoftManagerTests(object[] param)
		{
			microsoftManager = (MicrosoftApiManager)param[0];
			account = (IAccount)param[1];
		}

		#endregion Public Constructors

		#region Public Methods

		[Test()]
		public async Task GetChildrenTestAsync()
		{
			var root = await microsoftManager.GetDriveRootAsync(account).ConfigureAwait(false);
			await foreach (var child in microsoftManager.GetChildrenAsync(account, root.Id).ConfigureAwait(false)) {
				Console.WriteLine(child.Name);
				Assert.NotNull(child);
			}
		}

		[Test()]
		public async Task GetDriveRootTestAsync()
		{
			var root = await microsoftManager.GetDriveRootAsync(account).ConfigureAwait(false);
			Assert.NotNull(root);
		}

		[Test()]
		public async Task GetMeTestAsync()
		{
			var u = await microsoftManager.GetUserAsync(account).ConfigureAwait(false);
			Assert.NotNull(u);
		}

		[Test()]
		public void microsoftManagerTest()
		{
			Assert.NotNull(microsoftManager);
		}

		#endregion Public Methods
	}
}