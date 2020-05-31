using Cyc.MicrosoftApi;

using DriveExplorer.Models;
using DriveExplorer.Tests;

using Microsoft.Graph;
using Microsoft.Identity.Client;

using NUnit.Framework;

using System;
using System.IO;
using System.Threading.Tasks;

namespace DriveExplorer.ViewModels.Tests {

	[TestFixtureSource(typeof(TestSource))]
	public class ItemVMTests {

		#region Private Fields

		private readonly IAccount account;
		private readonly MicrosoftApiManager microsoftManager;
		private ItemVM localItem;
		private ItemVM onedriveItem;
		private DriveItem root;

		#endregion Private Fields

		#region Public Constructors

		public ItemVMTests(object[] param)
		{
			microsoftManager = (MicrosoftApiManager)param[0];
			account = (IAccount)param[1];
		}

		#endregion Public Constructors

		#region Public Methods

		[Test()]
		public async Task LocalItemSetIsExpandedAsyncTestAsync()
		{
			await localItem.SetIsExpandedAsync(true).ConfigureAwait(false);
			Assert.True(localItem.IsExpanded);
			Assert.NotNull(localItem.Children[0]);
		}

		[Test()]
		public async Task LocalItemSetIsSelectedAsyncTestAsync()
		{
			await localItem.SetIsSelectedAsync(true).ConfigureAwait(false);
			Assert.True(localItem.IsSelected);
		}

		[Test()]
		public void LocalItemVMTest()
		{
			Assert.NotNull(localItem);
		}

		[Test()]
		public async Task OneDriveItemSetIsExpandedAsyncTestAsync()
		{
			await onedriveItem.SetIsExpandedAsync(true).ConfigureAwait(false);
			Assert.True(onedriveItem.IsExpanded);
			Assert.NotNull(onedriveItem.Children[0]);
		}

		[Test()]
		public async Task OneDriveItemSetIsSelectedAsyncTestAsync()
		{
			await onedriveItem.SetIsSelectedAsync(true).ConfigureAwait(false);
			Assert.True(onedriveItem.IsSelected);
		}

		[Test()]
		public void OneDriveItemVMTest()
		{
			Assert.NotNull(onedriveItem);
		}

		[OneTimeSetUp]
		public async Task OneTimeSetupAsync()
		{
			root = await microsoftManager.GetDriveRootAsync(account).ConfigureAwait(false);
		}

		[SetUp]
		public void Setup()
		{
			var localRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(DriveExplorer));
			localItem = new ItemVM(new LocalItem(@"C:\\"), localRootPath);
			onedriveItem = new ItemVM(new OneDriveItem(microsoftManager, root, account), localRootPath);
		}

		#endregion Public Methods
	}
}