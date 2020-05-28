
using System;
using System.Collections.Generic;
using Google.Apis.Drive.v3.Data;

namespace DriveExplorer.Models {
	public class GoogleDriveItem : IItem {
		public ItemTypes Type { get; private set; }

		public string Name { get; private set; }

		public string FullPath { get; private set; }
		public User User { get; private set; }

		/// <summary>
		/// Root constructor
		/// </summary>
		public GoogleDriveItem(File root, User user) {
			Type = ItemTypes.GoogleDrive;
			Name = user.EmailAddress ?? user.DisplayName;
			FullPath = Name;
			User = user;
		}


		public IAsyncEnumerable<IItem> GetChildrenAsync() {
			throw new NotImplementedException();
		}
	}
}
