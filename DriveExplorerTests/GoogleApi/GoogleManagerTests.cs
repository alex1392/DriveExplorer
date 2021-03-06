﻿using Cyc.GoogleApi;

using DriveExplorer.Tests;

using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace DriveExplorer.GoogleApi.Tests {

	[TestFixture()]
	[TestFixtureSource(typeof(TestSource))]
	public class GoogleManagerTests {

		#region Private Fields

		private readonly GoogleApiManager googleManager;
		private readonly string userId;

		#endregion Private Fields

		#region Public Constructors

		public GoogleManagerTests(object[] param)
		{
			googleManager = (GoogleApiManager)param[2];
			userId = (string)param[4];
		}

		#endregion Public Constructors

		#region Public Methods

		[Test()]
		public async Task GetChildrenTestAsync()
		{
			var root = await googleManager.GetDriveRootAsync(userId).ConfigureAwait(false);
			await foreach (var child in googleManager.GetChildrenAsync(userId, root.Id).ConfigureAwait(false)) {
				Console.WriteLine(child.Name);
				Assert.NotNull(child);
			}
		}

		[Test()]
		public async Task GetDriveRootTestAsync()
		{
			var root = await googleManager.GetDriveRootAsync(userId).ConfigureAwait(false);
			Console.WriteLine(root.Id);
			Assert.NotNull(root);
		}

		[Test()]
		public async Task GetUserTestAsync()
		{
			var about = await googleManager.GetAboutAsync(userId).ConfigureAwait(false);
			Console.WriteLine(about.User.EmailAddress);
			Assert.NotNull(about);
		}

		[Test()]
		public void LoadAllUserIdTest()
		{
			var userIds = googleManager.LoadAllUserId();
			foreach (var id in userIds) {
				Console.WriteLine(id);
				Assert.NotNull(id);
			}
		}

		[Test()]
		public async Task UserLoginAsyncTestAsync()
		{
			var newUserId = await googleManager.UserLoginAsync(userId).ConfigureAwait(false);
			Assert.AreEqual(userId, newUserId);
		}

		[Ignore("This will change the test cache")]
		public async Task UserLogoutAsyncTestAsync()
		{
			await googleManager.UserLogoutAsync(userId).ConfigureAwait(false);
			Assert.IsFalse(googleManager.HasUser(userId));
		}

		#endregion Public Methods
	}
}