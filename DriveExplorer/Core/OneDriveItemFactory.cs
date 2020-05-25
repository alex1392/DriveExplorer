using DriveExplorer.IoC;
using DriveExplorer.MicrosoftApi;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.IO;
using System.Threading.Tasks;

namespace DriveExplorer {
    public class OneDriveItemFactory {
        public static IItem CreateRoot(DriveItem driveItem, User user) {
            var item = IocContainer.Default.GetTransient<OneDriveItem>();
            item.Id = driveItem.Id;
            item.Name = user.UserPrincipalName;
            item.Type = ItemTypes.OneDrive;
            item.FullPath = user.UserPrincipalName;
            item.UserId = user.Id;
            return item;
        }

        public static IItem CreateChild(DriveItem driveItem, OneDriveItem parent) {
            var item = IocContainer.Default.GetTransient<OneDriveItem>();
            item.Id = driveItem.Id;
            item.Name = driveItem.Name;
            item.Type = IsFolder(driveItem) ? ItemTypes.Folder :
                                ItemFactoryHelper.GetFileType(driveItem.Name);
            item.FullPath = Path.Combine(parent.FullPath, item.Name);
            item.UserId = parent.UserId;
            return item;
        }


        private static bool IsFolder(DriveItem driveItem) {
            return driveItem.Folder != null;
        }

    }
}
