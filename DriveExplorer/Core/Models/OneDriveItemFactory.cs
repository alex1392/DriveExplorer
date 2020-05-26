using DriveExplorer.MicrosoftApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DriveExplorer {
    public class OneDriveItemFactory {
        private readonly IServiceProvider serviceProvider;

        public OneDriveItemFactory(IServiceProvider serviceProvider) {
            this.serviceProvider = serviceProvider;
        }
        public IItem CreateRoot(DriveItem driveItem, User user) {
            var item = serviceProvider.GetService<OneDriveItem>();
            item.Id = driveItem.Id;
            item.Name = user.UserPrincipalName;
            item.Type = ItemTypes.OneDrive;
            item.FullPath = user.UserPrincipalName;
            item.UserId = user.Id;
            return item;
        }

        public IItem CreateChild(DriveItem driveItem, OneDriveItem parent) {
            var item = serviceProvider.GetService<OneDriveItem>();
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
