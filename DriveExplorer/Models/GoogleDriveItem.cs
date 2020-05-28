
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cyc.GoogleApi;
using Google.Apis.Drive.v3.Data;
using File = Google.Apis.Drive.v3.Data.File;

namespace DriveExplorer.Models {
	public class GoogleDriveItem : IItem {
		private readonly GoogleManager googleManager;

		public ItemTypes Type { get; private set; }

		public string Name { get; private set; }

		public string FullPath { get; private set; }
		public User User { get; private set; }
		public string Id { get; private set; }

		/// <summary>
		/// Root constructor
		/// </summary>
		public GoogleDriveItem(GoogleManager googleManager, About about, File file) {
			this.googleManager = googleManager;
			var user = about.User;
			Type = ItemTypes.GoogleDrive;
			Name = user.EmailAddress ?? user.DisplayName;
			FullPath = Name;
			User = user;
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
			User = parent.User;
			Id = child.Id;
		}

		private bool IsFolder(File child) {
			// application/vnd.google-apps.folder
			return child.MimeType.Contains("folder");
		}

		public async IAsyncEnumerable<IItem> GetChildrenAsync() {
			await foreach (var child in googleManager.GetChildrenAsync(Id).ConfigureAwait(false)) {
				yield return new GoogleDriveItem(this, child);
			}
		}
	}
}
