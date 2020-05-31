using DriveExplorer.ViewModels;
using DriveExplorer.Tests;

using NUnit.Framework;

using System.Threading.Tasks;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DriveExplorer.ViewModels.Tests {
	[TestFixture()]
	[TestFixtureSource(typeof(TestSource))]
	public class MainWindowVMTests {
		private readonly MainWindowVM mainWindowVM;
		private ObservableCollection<ItemVM> treeItemVMs;

		public MainWindowVMTests(object[] param)
		{
			mainWindowVM = (MainWindowVM)param[3];
		}
		[OneTimeSetUp]
		public async Task OneTimeSetupAsync()
		{
			await mainWindowVM.InitializeAsync().ConfigureAwait(false);
			treeItemVMs = mainWindowVM.TreeItemVMs;
			mainWindowVM.TreeItemVMs.Clear(); // avoid duplicate in setup
		}

		[SetUp]
		public void Setup()
		{
			foreach (var vm in treeItemVMs) {
				mainWindowVM.TreeItemVMs.Add(vm.Clone());
			}
		}

		[TearDown]
		public void TearDown()
		{
			mainWindowVM.Reset();
		}

		[Test()]
		public void MainWindowVMTest()
		{
			Assert.NotNull(mainWindowVM);
		}

		[Test()]
		public async Task TreeItem_SelectedAsyncTestAsync()
		{
			var itemVM = mainWindowVM.TreeItemVMs[0];
			await mainWindowVM.TreeItemSelectedAsync(itemVM).ConfigureAwait(false);
			Assert.That(mainWindowVM.TreeItemVMs[0].Children[0] != null);
			Assert.That(mainWindowVM.CurrentItemVMs.Count > 0);
		}

		[Test()]
		public async Task CurrentItem_SelectedAsyncTestAsync()
		{
			await mainWindowVM.TreeItemVMs[0].SetIsSelectedAsync(true).ConfigureAwait(false); // fill current items
			var itemVM = mainWindowVM.CurrentItemVMs[0];
			await mainWindowVM.CurrentItemSelectedAsync(itemVM).ConfigureAwait(false);
			Assert.That(mainWindowVM.TreeItemVMs[0].Children[0].Children[0] != null);
		}

		[Test()]
		public void ResetTest()
		{
			mainWindowVM.Reset();
			Assert.That(mainWindowVM.TreeItemVMs.Count == 0);
			Assert.That(mainWindowVM.CurrentItemVMs.Count == 0);
		}

		[Test()]
		public async Task InitializeAsyncTestAsync()
		{
			mainWindowVM.Reset();
			await mainWindowVM.InitializeAsync().ConfigureAwait(false);
			Assert.That(mainWindowVM.TreeItemVMs.Count == 3);
		}

		[Test()]
		public void LoginOneDriveAsyncTest()
		{
			throw new NotImplementedException();
		}

		[Test()]
		public async Task LogoutOneDriveAsyncTestAsync()
		{
			var originalCount = mainWindowVM.TreeItemVMs.Count;
			await mainWindowVM.LogoutOneDriveAsync(mainWindowVM.TreeItemVMs.First(vm => vm.Item.Type == Models.ItemTypes.OneDrive).Item).ConfigureAwait(false);
			var nowCount = mainWindowVM.TreeItemVMs.Count;
			Assert.That(nowCount < originalCount);
		}

		[Ignore("This requires user interaction")]
		public async Task LoginGoogleDriveAsyncTestAsync()
		{
			await mainWindowVM.LoginGoogleDriveAsync().ConfigureAwait(false);
			Assert.True(mainWindowVM.TreeItemVMs.Count > 0);
			Assert.True(mainWindowVM.CurrentItemVMs.Count > 0);
		}

		[Test()]
		public async Task LogoutGoogleDriveAsyncTestAsync()
		{
			var originalCount = mainWindowVM.TreeItemVMs.Count;
			await mainWindowVM.LogoutGoogleDriveAsync(mainWindowVM.TreeItemVMs.First(vm => vm.Item.Type == Models.ItemTypes.GoogleDrive).Item).ConfigureAwait(false);
			var nowCount = mainWindowVM.TreeItemVMs.Count;
			Assert.That(nowCount < originalCount);
		}
	}
}