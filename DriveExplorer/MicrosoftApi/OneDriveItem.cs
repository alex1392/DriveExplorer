using DriveExplorer.Models;

using Microsoft.Graph;
using Microsoft.Identity.Client;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DriveExplorer.MicrosoftApi {
	public class OneDriveItem : IItem {
		private readonly GraphManager graphManager;

		public ItemTypes Type { get; private set; }
		public string Name { get; private set; }
		public string FullPath { get; private set; }
		public string Id { get; private set; }
		public IAccount UserAccount { get; private set; }

		/// <summary>
		/// Constructor of root item
		/// </summary>
		public OneDriveItem(GraphManager graphManager, DriveItem driveItem, IAccount account) {
			this.graphManager = graphManager;
			Id = driveItem.Id;
			Name = account.Username;
			Type = ItemTypes.OneDrive;
			FullPath = account.Username;
			UserAccount = account;
		}
		public async IAsyncEnumerable<IItem> GetChildrenAsync() {
			await foreach (var item in graphManager.GetChildrenAsync(Id, UserAccount).ConfigureAwait(false)) {
				yield return GetChild(item, this);
			}
		}

		private OneDriveItem(GraphManager graphManager) {
			this.graphManager = graphManager;
		}
		private OneDriveItem GetChild(DriveItem driveItem, OneDriveItem parent) {
			return new OneDriveItem(parent.graphManager)
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
