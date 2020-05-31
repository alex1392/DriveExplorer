using Cyc.MicrosoftApi;

using DriveExplorer.Tests;

using Microsoft.Graph;
using Microsoft.Identity.Client;

using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace DriveExplorer.Models.Tests {

	[TestFixtureSource(typeof(TestSource))]
	public class OneDriveItemTests {

		#region Private Fields

		private readonly IAccount account;
		private readonly MicrosoftApiManager microsoftManager;
		private DriveItem root;

		#endregion Private Fields

		#region Public Constructors

		public OneDriveItemTests(object[] param)
		{
			microsoftManager = (MicrosoftApiManager)param[0];
			account = (IAccount)param[1];
		}

		#endregion Public Constructors

		#region Public Methods

		[Test()]
		public async Task GetChildrenAsyncTestAsync()
		{
			var item = new OneDriveItem(microsoftManager, root, account);
			await foreach (var child in item.GetChildrenAsync().ConfigureAwait(false)) {
				Console.WriteLine(child.Name);
				Assert.NotNull(child);
			}
		}

		[Test()]
		public void OneDriveItemTest()
		{
			var item = new OneDriveItem(microsoftManager, root, account);
			Assert.NotNull(item);
		}

		[OneTimeSetUp]
		public async Task OneTimeSetupAsync()
		{
			root = await microsoftManager.GetDriveRootAsync(account).ConfigureAwait(false);
		}

		#endregion Public Methods
	}
}