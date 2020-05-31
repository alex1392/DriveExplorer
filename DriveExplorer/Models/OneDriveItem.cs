
using Cyc.MicrosoftApi;

using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DriveExplorer.Models {
	public class OneDriveItem : IItem {
		private readonly MicrosoftApiManager microsoftApiManager;

		public ItemTypes Type { get; private set; }
		public string Name { get; private set; }
		public string FullPath { get; private set; }
		public string Id { get; private set; }
		public IAccount UserAccount { get; private set; }

		public long? Size { get; private set; }

		public DateTimeOffset? LastModifiedTime { get; private set; }
		/// <summary>
		/// Root constructor
		/// </summary>
		public OneDriveItem(MicrosoftApiManager microsoftApiManager, DriveItem driveItem, IAccount account)
		{
			this.microsoftApiManager = microsoftApiManager;
			Name = account.Username;
			Type = ItemTypes.OneDrive;
			FullPath = account.Username;
			UserAccount = account;
			Id = driveItem.Id;
			Size = 0;
			LastModifiedTime = driveItem.LastModifiedDateTime;
		}
		/// <summary>
		/// Child constructor
		/// </summary>
		public OneDriveItem(DriveItem driveItem, OneDriveItem parent)
		{
			microsoftApiManager = parent.microsoftApiManager;
			Name = driveItem.Name;
			Type = IsFolder(driveItem) ? ItemTypes.Folder : ItemTypes.File;
			FullPath = Path.Combine(parent.FullPath, driveItem.Name);
			UserAccount = parent.UserAccount;
			Id = driveItem.Id;
			Size = driveItem.Size ?? 0;
			LastModifiedTime = driveItem.LastModifiedDateTime;
		}

		public async Task DownloadAsync(string localPath)
		{
			using var stream = await microsoftApiManager.GetFileContentAsync(UserAccount, Id).ConfigureAwait(false);
			using var fileStream = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.Write);
			stream.Seek(0, SeekOrigin.Begin);
			stream.CopyTo(fileStream);
		}

		public async IAsyncEnumerable<IItem> GetChildrenAsync()
		{
			await foreach (var item in microsoftApiManager.GetChildrenAsync(UserAccount, Id).ConfigureAwait(false)) {
				yield return new OneDriveItem(item, this);
			}
		}

		private static bool IsFolder(DriveItem driveItem)
		{
			return driveItem.Folder != null;
		}


	}
}
