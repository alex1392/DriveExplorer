﻿using Cyc.GoogleApi;
using Cyc.Standard;

using Google.Apis.Drive.v3.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DriveExplorer.Models {

	public class GoogleDriveManager : IDriveManager {

		#region Private Fields

		private readonly GoogleApiManager googleManager;
		private readonly ILogger logger;
		private readonly Dictionary<string, User> userRegistry = new Dictionary<string, User>();

		#endregion Private Fields

		#region Public Events

		public event EventHandler BeforeTaskExecuted {
			add => googleManager.BeforeTaskExecute += value;
			remove => googleManager.BeforeTaskExecute -= value;
		}

		public event EventHandler<IItem> LoginCompleted;

		public event EventHandler<IItem> LogoutCompleted;

		public event EventHandler TaskExecuted {
			add => googleManager.TaskExecuted += value;
			remove => googleManager.TaskExecuted -= value;
		}

		#endregion Public Events

		#region Public Constructors

		public GoogleDriveManager(ILogger logger, GoogleApiManager googleManager)
		{
			this.logger = logger;
			this.googleManager = googleManager;
		}

		#endregion Public Constructors

		#region Public Methods

		public async Task AutoLoginAsync()
		{
			if (googleManager == null) {
				return;
			}
			foreach (var userId in googleManager.LoadAllUserId()) {
				await googleManager.UserLoginAsync(userId).ConfigureAwait(true);
				await CreateGoogleDrive(userId).ConfigureAwait(true);
			}
		}

		public async Task LoginAsync()
		{
			if (googleManager == null) {
				return;
			}
			var userId = await googleManager.UserLoginAsync().ConfigureAwait(true);
			await CreateGoogleDrive(userId).ConfigureAwait(true);
		}

		public async Task LoginAsync(CancellationToken token)
		{
			if (googleManager == null) {
				return;
			}
			var userId = await googleManager.UserLoginAsync(token).ConfigureAwait(true);
			await CreateGoogleDrive(userId).ConfigureAwait(true);
		}

		public async Task LogoutAsync(IItem item)
		{
			if (googleManager == null) {
				return;
			}
			if (!(item is GoogleDriveItem googleDriveItem)) {
				throw new InvalidOperationException();
			}
			if (await googleManager.UserLogoutAsync(googleDriveItem.UserId).ConfigureAwait(true)) {
				userRegistry.Remove(googleDriveItem.UserId);
				LogoutCompleted?.Invoke(this, item);
			}
		}

		#endregion Public Methods

		#region Private Methods

		private async Task CreateGoogleDrive(string userId)
		{
			if (userId == null) {
				return;
			}
			var about = await googleManager.GetAboutAsync(userId).ConfigureAwait(true);
			if (about == null) {
				return;
			}
			if (HasUser(about.User)) {
				logger?.Log("User has already logged in.");
				await googleManager.UserLogoutAsync(userId).ConfigureAwait(false);
				return;
			}
			var root = await googleManager.GetDriveRootAsync(userId).ConfigureAwait(true);
			if (root == null) {
				return;
			}
			userRegistry.Add(userId, about.User);
			var item = new GoogleDriveItem(googleManager, about, root, userId);
			LoginCompleted?.Invoke(this, item);
		}

		private bool HasUser(User user)
		{
			return userRegistry.Any(pair => pair.Value.EmailAddress == user.EmailAddress);
		}

		#endregion Private Methods
	}
}