using Cyc.GoogleApi;
using Cyc.Standard;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DriveExplorer.Models {
	public class GoogleDriveManager : IDriveManager {
		private readonly ILogger logger;
		private readonly GoogleApiManager googleManager;

		public event EventHandler<IItem> LoginCompleted;
		public event EventHandler<IItem> LogoutCompleted;
		public event EventHandler BeforeTaskExecuted {
			add => googleManager.BeforeTaskExecute += value;
			remove => googleManager.BeforeTaskExecute -= value;
		}
		public event EventHandler TaskExecuted {
			add => googleManager.TaskExecuted += value;
			remove => googleManager.TaskExecuted -= value;
		}
		public GoogleDriveManager(ILogger logger, GoogleApiManager googleManager)
		{
			this.logger = logger;
			this.googleManager = googleManager;
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
		public async Task LogoutAsync(IItem item)
		{
			if (googleManager == null) {
				return;
			}
			if (!(item is GoogleDriveItem googleDriveItem)) {
				throw new InvalidOperationException();
			}
			if (await googleManager.UserLogoutAsync(googleDriveItem.UserId).ConfigureAwait(true)) {
				LogoutCompleted?.Invoke(this, item);
			}
		}
		private async Task CreateGoogleDrive(string userId)
		{
			if (userId == null) {
				return;
			}
			var about = await googleManager.GetAboutAsync(userId).ConfigureAwait(true);
			if (about == null) {
				return;
			}
			var root = await googleManager.GetDriveRootAsync(userId).ConfigureAwait(true);
			if (root == null) {
				return;
			}
			var item = new GoogleDriveItem(googleManager, about, root, userId);
			LoginCompleted?.Invoke(this, item);
		}

	}
}
