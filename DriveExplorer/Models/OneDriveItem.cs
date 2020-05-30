
using Cyc.MicrosoftApi;

using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DriveExplorer.Models {
	public class OneDriveItem : IItem {
		public class Factory : ItemFactoryBase {
			public override IItem CreateRoot(params object[] parameters)
			{
				var microsoftApiManager = (MicrosoftApiManager)parameters[0];
				var driveItem = (DriveItem)parameters[1];
				var account = (IAccount)parameters[2];
				var item = new OneDriveItem(microsoftApiManager, this, driveItem)
				{
					Name = account.Username,
					Type = ItemTypes.OneDrive,
					FullPath = account.Username,
					UserAccount = account,
				};
				return item;
			}

			public override IItem CreateFile(params object[] parameters)
			{
				var driveItem = (DriveItem)parameters[0];
				var parent = (OneDriveItem)parameters[1];
				var item = new OneDriveItem(parent.microsoftApiManager, this, driveItem)
				{
					Name = driveItem.Name,
					Type = GetFileType(driveItem.Name),
					FullPath = Path.Combine(parent.FullPath, driveItem.Name),
					UserAccount = parent.UserAccount
				};
				return item;
			}

			public override IItem CreateFolder(params object[] parameters)
			{
				var driveItem = (DriveItem)parameters[0];
				var parent = (OneDriveItem)parameters[1];
				var item = new OneDriveItem(parent.microsoftApiManager, this, driveItem)
				{
					Name = driveItem.Name,
					Type = ItemTypes.Folder,
					FullPath = Path.Combine(parent.FullPath, driveItem.Name),
					UserAccount = parent.UserAccount
				};
				return item;
			}
		}

		private readonly MicrosoftApiManager microsoftApiManager;
		private readonly Factory factory;

		public ItemTypes Type { get; private set; }
		public string Name { get; private set; }
		public string FullPath { get; private set; }
		public string Id { get; private set; }
		public IAccount UserAccount { get; private set; }

		public long? Size { get; private set; }

		public DateTimeOffset? LastModifiedTime { get; private set; }

		public OneDriveItem(MicrosoftApiManager microsoftManager, Factory factory, DriveItem driveItem)
		{
			this.microsoftApiManager = microsoftManager;
			this.factory = factory;
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
				if (IsFolder(item)) {
					yield return factory.CreateFolder(item, this);

				} else {
					yield return factory.CreateFile(item, this);
				}
			}
		}

		private static bool IsFolder(DriveItem driveItem)
		{
			return driveItem.Folder != null;
		}


	}
}
