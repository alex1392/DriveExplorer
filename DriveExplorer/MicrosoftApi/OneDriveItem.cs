using DriveExplorer.Models;

using Microsoft.Graph;
using Microsoft.Identity.Client;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DriveExplorer.MicrosoftApi {
	public class OneDriveItem : IItem {
		private readonly MicrosoftManager microsoftManager;

		public ItemTypes Type { get; private set; }
		public string Name { get; private set; }
		public string FullPath { get; private set; }
		public string Id { get; private set; }
		public IAccount UserAccount { get; private set; }

		/// <summary>
		/// Constructor of root item
		/// </summary>
		public OneDriveItem(MicrosoftManager microsoftManager, DriveItem driveItem, IAccount account) {
			this.microsoftManager = microsoftManager;
			Id = driveItem.Id;
			Name = account.Username;
			Type = ItemTypes.OneDrive;
			FullPath = account.Username;
			UserAccount = account;
		}
		public async IAsyncEnumerable<IItem> GetChildrenAsync() {
			await foreach (var item in microsoftManager.GetChildrenAsync(Id, UserAccount).ConfigureAwait(false)) {
				yield return GetChild(item, this);
			}
		}

		private OneDriveItem(MicrosoftManager microsoftManager) {
			this.microsoftManager = microsoftManager;
		}
		private OneDriveItem GetChild(DriveItem driveItem, OneDriveItem parent) {
			return new OneDriveItem(parent.microsoftManager)
			{
				Id = driveItem.Id,
				Name = driveItem.Name,
				Type = IsFolder(driveItem) ? ItemTypes.Folder : ItemFactoryHelper.GetFileType(driveItem.Name),
				FullPath = Path.Combine(parent.FullPath, driveItem.Name),
				UserAccount = parent.UserAccount
			};
		}	

		private static bool IsFolder(DriveItem driveItem) {
			return driveItem.Folder != null;
		}


	}
}
