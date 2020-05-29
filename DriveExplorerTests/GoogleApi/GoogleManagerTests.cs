using Cyc.GoogleApi;

using DriveExplorer.Tests;

using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace DriveExplorer.GoogleApi.Tests {
	[TestFixture()]
	[TestFixtureSource(typeof(TestSource))]
	public class GoogleManagerTests {
		private readonly GoogleManager googleManager;
		private readonly string userId;

		public GoogleManagerTests(object[] param) {
			googleManager = (GoogleManager)param[2];
			userId = (string)param[4];
		}

		[Test()]
		public async Task GetUserTestAsync() {
			var about = await googleManager.GetAboutAsync(userId).ConfigureAwait(false);
			Console.WriteLine(about.User.EmailAddress);
			Assert.NotNull(about);
		}

		[Test()]
		public async Task GetDriveRootTestAsync() {
			var root = await googleManager.GetDriveRootAsync(userId).ConfigureAwait(false);
			Console.WriteLine(root.Id);
			Assert.NotNull(root);
		}

		[Test()]
		public async Task GetChildrenTestAsync() {
			var root = await googleManager.GetDriveRootAsync(userId).ConfigureAwait(false);
			await foreach (var child in googleManager.GetChildrenAsync(userId, root.Id).ConfigureAwait(false)) {
				Console.WriteLine(child.Name);
				Assert.NotNull(child);
			}
		}

		[Test()]
		public async Task UserLoginAsyncTestAsync() {
			var newUserId = await googleManager.UserLoginAsync(userId).ConfigureAwait(false);
			Assert.AreEqual(userId, newUserId);
		}

		[Test()]
		public async Task UserLogoutAsyncTestAsync() {
			await googleManager.UserLogoutAsync(userId).ConfigureAwait(false);
			Assert.IsFalse(googleManager.HasUser(userId));
		}


		[Test()]
		public void LoadAllUserIdTest() {
			var userIds = googleManager.LoadAllUserId();
			foreach (var id in userIds) {
				Console.WriteLine(id);
				Assert.NotNull(id);
			}
		}
	}
}