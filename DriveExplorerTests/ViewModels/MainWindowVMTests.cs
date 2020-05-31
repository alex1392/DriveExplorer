using DriveExplorer.Tests;

using NUnit.Framework;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DriveExplorer.ViewModels.Tests {

	[TestFixture()]
	[TestFixtureSource(typeof(TestSource))]
	public class MainWindowVMTests {

		#region Private Fields

		private readonly MainWindowVM mainWindowVM;
		private ObservableCollection<ItemVM> treeItemVMs = new ObservableCollection<ItemVM>();

		#endregion Private Fields

		#region Public Constructors

		public MainWindowVMTests(object[] param)
		{
			mainWindowVM = (MainWindowVM)param[3];
		}

		#endregion Public Constructors

		#region Public Methods

		[Test()]
		public async Task CurrentItem_SelectedAsyncTestAsync()
		{
			await mainWindowVM.TreeItemSelectedAsync(mainWindowVM.TreeItemVMs[0]).ConfigureAwait(false);// fill current items
			var itemVM = mainWindowVM.CurrentItemVMs[0];
			await mainWindowVM.CurrentItemSelectedAsync(itemVM).ConfigureAwait(false);
			Assert.That(mainWindowVM.TreeItemVMs[0].Children[0].Children[0] != null);
		}

		[Test()]
		public async Task InitializeAsyncTestAsync()
		{
			mainWindowVM.Reset();
			await mainWindowVM.InitializeAsync().ConfigureAwait(false);
			Assert.That(mainWindowVM.TreeItemVMs.Count == 3);
		}

		[Ignore("This requires user interaction")]
		public async Task LoginGoogleDriveAsyncTestAsync()
		{
			await mainWindowVM.LoginGoogleDriveAsync().ConfigureAwait(false);
			Assert.True(mainWindowVM.TreeItemVMs.Count > 0);
			Assert.True(mainWindowVM.CurrentItemVMs.Count > 0);
		}

		[Ignore("This method requires user interaction.")]
		public async Task LoginOneDriveAsyncTestAsync()
		{
			var originalCount = mainWindowVM.TreeItemVMs.Count;
			await mainWindowVM.LoginOneDriveAsync().ConfigureAwait(false);
			Assert.That(mainWindowVM.TreeItemVMs.Count > originalCount);
		}

		[Ignore("This method would change the cache.")]
		public async Task LogoutGoogleDriveAsyncTestAsync()
		{
			var originalCount = mainWindowVM.TreeItemVMs.Count;
			await mainWindowVM.LogoutGoogleDriveAsync(mainWindowVM.TreeItemVMs.First(vm => vm.Item.Type == Models.ItemTypes.GoogleDrive).Item).ConfigureAwait(false);
			var nowCount = mainWindowVM.TreeItemVMs.Count;
			Assert.That(nowCount < originalCount);
		}

		[Test()]
		public async Task LogoutOneDriveAsyncTestAsync()
		{
			var originalCount = mainWindowVM.TreeItemVMs.Count;
			await mainWindowVM.LogoutOneDriveAsync(mainWindowVM.TreeItemVMs.First(vm => vm.Item.Type == Models.ItemTypes.OneDrive).Item).ConfigureAwait(false);
			var nowCount = mainWindowVM.TreeItemVMs.Count;
			Assert.That(nowCount < originalCount);
		}

		[Test()]
		public void MainWindowVMTest()
		{
			Assert.NotNull(mainWindowVM);
		}

		[OneTimeSetUp]
		public async Task OneTimeSetupAsync()
		{
			await mainWindowVM.InitializeAsync().ConfigureAwait(false);
			foreach (var vm in mainWindowVM.TreeItemVMs) {
				treeItemVMs.Add(vm);
			}
			mainWindowVM.TreeItemVMs.Clear(); // avoid duplicate in setup
		}

		[Test()]
		public void ResetTest()
		{
			mainWindowVM.Reset();
			Assert.That(mainWindowVM.TreeItemVMs.Count == 0);
			Assert.That(mainWindowVM.CurrentItemVMs.Count == 0);
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
		public async Task TreeItem_SelectedAsyncTestAsync()
		{
			var itemVM = mainWindowVM.TreeItemVMs[0];
			await mainWindowVM.TreeItemSelectedAsync(itemVM).ConfigureAwait(false);
			Assert.That(mainWindowVM.TreeItemVMs[0].Children[0] != null);
			Assert.That(mainWindowVM.CurrentItemVMs.Count > 0);
		}

		#endregion Public Methods
	}
}