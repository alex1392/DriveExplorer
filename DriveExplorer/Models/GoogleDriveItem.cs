using Cyc.GoogleApi;

using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using File = Google.Apis.Drive.v3.Data.File;

namespace DriveExplorer.Models {
	public class GoogleDriveItem : IItem {
		private readonly GoogleManager googleManager;

		public ItemTypes Type { get; private set; }

		public string Name { get; private set; }

		public string FullPath { get; private set; }
		public string UserId { get; private set; }
		public string Id { get; private set; }

		public long Size { get; private set; }

		public DateTime LastModifiedDate { get; private set; }

		/// <summary>
		/// Root constructor
		/// </summary>
		public GoogleDriveItem(GoogleManager googleManager, About about, File file, string userId) {
			this.googleManager = googleManager;
			var user = about.User;
			Type = ItemTypes.GoogleDrive;
			Name = user.EmailAddress;
			FullPath = Name;
			UserId = userId;
			Id = file.Id;
		}
		/// <summary>
		/// Child constructor
		/// </summary>
		private GoogleDriveItem(GoogleDriveItem parent, File child) {
			googleManager = parent.googleManager;
			Type = IsFolder(child) ?
				ItemTypes.Folder :
				ItemFactoryHelper.GetFileType(child.Name);
			Name = child.Name;
			FullPath = Path.Combine(parent.FullPath, child.Name);
			UserId = parent.UserId;
			Id = child.Id;
		}

		private bool IsFolder(File child) {
			// application/vnd.google-apps.folder
			return child.MimeType.Contains("folder");
		}

		public async IAsyncEnumerable<IItem> GetChildrenAsync() {
			await foreach (var child in googleManager.GetChildrenAsync(UserId, Id).ConfigureAwait(false)) {
				yield return new GoogleDriveItem(this, child);
			}
		}
	}
}
