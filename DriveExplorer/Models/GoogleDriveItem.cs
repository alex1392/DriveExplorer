using Cyc.GoogleApi;
using Google.Apis.Download;
using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using File = Google.Apis.Drive.v3.Data.File;
namespace DriveExplorer.Models {
	public class GoogleDriveItem : IItem {
		public class Factory : ItemFactoryBase {
			public override IItem CreateRoot(params object[] parameters) {
				var googleApiManager = (GoogleApiManager)parameters[0];
				var about = (About)parameters[1];
				var driveItem = (File)parameters[2];
				var userId = (string)parameters[3];
				var user = about.User;
				var item = new GoogleDriveItem(googleApiManager, this, driveItem)
				{
					Type = ItemTypes.GoogleDrive,
					Name = user.EmailAddress,
					FullPath = user.EmailAddress,
					UserId = userId,
				};
				return item;
			}
			public override IItem CreateFile(params object[] parameters) {
				var parent = (GoogleDriveItem)parameters[0];
				var driveItem = (File)parameters[1];
				var item = new GoogleDriveItem(parent.googleApiManager, this, driveItem)
				{
					Type = GetFileType(driveItem.Name),
					Name = driveItem.Name,
					FullPath = Path.Combine(parent.FullPath, driveItem.Name),
					UserId = parent.UserId,
				};
				return item;
			}
			public override IItem CreateFolder(params object[] parameters) {
				var parent = (GoogleDriveItem)parameters[0];
				var driveItem = (File)parameters[1];
				var item = new GoogleDriveItem(parent.googleApiManager, this, driveItem)
				{
					Type = ItemTypes.Folder,
					Name = driveItem.Name,
					FullPath = Path.Combine(parent.FullPath, driveItem.Name),
					UserId = parent.UserId,
				};
				return item;
			}
		}
		private readonly GoogleApiManager googleApiManager;
		private readonly Factory factory;
		public ItemTypes Type { get; private set; }
		public string Name { get; private set; }
		public string FullPath { get; private set; }
		public string UserId { get; private set; }
		public string Id { get; private set; }
		public long? Size { get; private set; }
		public DateTimeOffset? LastModifiedTime { get; private set; }
		public GoogleDriveItem(GoogleApiManager googleApiManager, Factory factory, File driveItem) {
			this.googleApiManager = googleApiManager;
			this.factory = factory;
			Id = driveItem.Id;
			Size = driveItem.Size;
			LastModifiedTime = driveItem.ModifiedTime;
		}
		
		private bool IsFolder(File child) {
			// application/vnd.google-apps.folder
			return child.MimeType.Contains("folder");
		}
		public async IAsyncEnumerable<IItem> GetChildrenAsync() {
			await foreach (var child in googleApiManager.GetChildrenAsync(UserId, Id).ConfigureAwait(false)) {
				if (IsFolder(child)) {
					yield return factory.CreateFolder(this, child);
				} else {
					yield return factory.CreateFile(this, child);
				}
			}
		}

		public async Task DownloadAsync(string localPath)
		{
			await googleApiManager.DownloadAsync(UserId, Id, localPath).ConfigureAwait(false);
		}
	}
}
