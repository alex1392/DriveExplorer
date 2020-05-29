
using Cyc.MicrosoftApi;

using Microsoft.Graph;
using Microsoft.Identity.Client;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DriveExplorer.Models {
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
		/// <summary>
		/// Constructor of child
		/// </summary>
		private OneDriveItem(DriveItem driveItem, OneDriveItem parent) {
			microsoftManager = parent.microsoftManager;
			Id = driveItem.Id;
			Name = driveItem.Name;
			Type = IsFolder(driveItem) ? ItemTypes.Folder : ItemFactoryHelper.GetFileType(driveItem.Name);
			FullPath = Path.Combine(parent.FullPath, driveItem.Name);
			UserAccount = parent.UserAccount;
		}

		public async IAsyncEnumerable<IItem> GetChildrenAsync() {
			await foreach (var item in microsoftManager.GetChildrenAsync(UserAccount, Id).ConfigureAwait(false)) {
				yield return new OneDriveItem(item, this);
			}
		}

		private static bool IsFolder(DriveItem driveItem) {
			return driveItem.Folder != null;
		}


	}
}
