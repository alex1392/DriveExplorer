using Microsoft.Graph;
using DriveExplorer.MicrosoftApi;
using DriveExplorer.IoC;
using System.IO;

namespace DriveExplorer {
    public class OneDriveItemFactory {
        private readonly GraphManager graphManager;
        public OneDriveItemFactory(GraphManager graphManager) {
            this.graphManager = graphManager;
        }

        public IItem Create(DriveItem driveItem) {
            var item = IocContainer.Default.GetTransient<OneDriveItem>();
            item.Id = driveItem.Id;
            var isRoot = IsRoot(driveItem);
            item.Name = isRoot ? graphManager.UserCache.UserPrincipalName : driveItem.Name;
            item.Type = isRoot ? ItemTypes.Drive :
                            IsFolder(driveItem) ? ItemTypes.Folder :
                                ItemFactoryHelper.GetFileType(driveItem.Name);
            item.FullPath = isRoot ? graphManager.UserCache.UserPrincipalName : GetFilePath(driveItem);
            return item;

            string GetFilePath(DriveItem driveItem) {
                var path = driveItem.ParentReference.Path
                    .Replace("/drive/root:", graphManager.UserCache.UserPrincipalName)
                    .Replace('/', Path.DirectorySeparatorChar);
                path = Path.Combine(path, item.Name);
                return path;
            }
        }


        private bool IsFolder(DriveItem driveItem) {
            return driveItem.Folder != null;
        }

        private bool IsRoot(DriveItem driveItem) {
            return driveItem.Name == "root" && driveItem.ParentReference.Path == null;
        }
    }
}
