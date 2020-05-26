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
		public string UserId { get; private set; }
		public IAccount UserAccount { get; }

		/// <summary>
		/// Constructor of root item
		/// </summary>
		public OneDriveItem(GraphManager graphManager, DriveItem driveItem, User user, IAccount userAccount) {
			this.graphManager = graphManager;
			Id = driveItem.Id;
			Name = user.UserPrincipalName;
			Type = ItemTypes.OneDrive;
			FullPath = user.UserPrincipalName;
			UserId = user.Id;
			UserAccount = userAccount;
		}

		/// <summary>
		/// Constructor of child item
		/// </summary>
		public OneDriveItem(DriveItem driveItem, OneDriveItem parent) {
			graphManager = parent.graphManager;
			Id = driveItem.Id;
			Name = driveItem.Name;
			Type = IsFolder(driveItem) ? ItemTypes.Folder : ItemFactoryHelper.GetFileType(driveItem.Name);
			FullPath = Path.Combine(parent.FullPath, driveItem.Name);
			UserId = parent.UserId;
			UserAccount = parent.UserAccount;
		}

		private static bool IsFolder(DriveItem driveItem) {
			return driveItem.Folder != null;
		}


		public async IAsyncEnumerable<IItem> GetChildrenAsync() {
			await foreach (var item in graphManager.GetChildrenAsync(Id, UserId).ConfigureAwait(false)) {
				yield return new OneDriveItem(item, this);
			}
		}
	}
}
