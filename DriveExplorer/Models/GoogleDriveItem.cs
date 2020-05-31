using Cyc.GoogleApi;

using Google.Apis.Drive.v3.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using File = Google.Apis.Drive.v3.Data.File;

namespace DriveExplorer.Models {

	public class GoogleDriveItem : IItem {

		#region Private Fields

		private readonly GoogleApiManager googleApiManager;

		#endregion Private Fields

		#region Public Properties

		public string FullPath { get; private set; }
		public string Id { get; private set; }
		public DateTimeOffset? LastModifiedTime { get; private set; }
		public string Name { get; private set; }
		public long? Size { get; private set; }
		public ItemTypes Type { get; private set; }
		public string UserId { get; private set; }

		#endregion Public Properties

		#region Public Constructors

		/// <summary>
		/// Root constructor
		/// </summary>
		public GoogleDriveItem(GoogleApiManager googleApiManager, About about, File driveItem, string userId)
		{
			var user = about.User;
			this.googleApiManager = googleApiManager;
			Type = ItemTypes.GoogleDrive;
			Name = user.EmailAddress;
			FullPath = user.EmailAddress;
			UserId = userId;
			Id = driveItem.Id;
			Size = driveItem.Size;
			LastModifiedTime = driveItem.ModifiedTime;
		}

		/// <summary>
		/// Child constructor
		/// </summary>
		public GoogleDriveItem(GoogleDriveItem parent, File driveItem)
		{
			googleApiManager = parent.googleApiManager;
			Type = IsFolder(driveItem) ? ItemTypes.Folder : ItemTypes.File;
			Name = driveItem.Name;
			FullPath = Path.Combine(parent.FullPath, driveItem.Name);
			UserId = parent.UserId;
			Id = driveItem.Id;
			Size = driveItem.Size;
			LastModifiedTime = driveItem.ModifiedTime;
		}

		#endregion Public Constructors

		#region Public Methods

		public async Task DownloadAsync(string localPath)
		{
			await googleApiManager.DownloadAsync(UserId, Id, localPath).ConfigureAwait(false);
		}

		public async IAsyncEnumerable<IItem> GetChildrenAsync()
		{
			await foreach (var child in googleApiManager.GetChildrenAsync(UserId, Id).ConfigureAwait(false)) {
				yield return new GoogleDriveItem(this, child);
			}
		}

		#endregion Public Methods

		#region Private Methods

		private bool IsFolder(File child)
		{
			// application/vnd.google-apps.folder
			return child.MimeType.Contains("folder");
		}

		#endregion Private Methods
	}
}