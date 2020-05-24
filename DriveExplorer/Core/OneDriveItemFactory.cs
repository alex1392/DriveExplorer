using DriveExplorer.IoC;
using DriveExplorer.MicrosoftApi;
using Microsoft.Graph;
using System.IO;
using System.Threading.Tasks;

namespace DriveExplorer {
    public class OneDriveItemFactory {
        private readonly GraphManager graphManager;
        public OneDriveItemFactory(GraphManager graphManager) {
            this.graphManager = graphManager;
        }

        public async Task<IItem> CreateRootAsync(DriveItem driveItem) {
            var item = IocContainer.Default.GetTransient<OneDriveItem>();
            item.Id = driveItem.Id;
            var user = await graphManager.GetMeAsync().ConfigureAwait(false);
            item.Name = user?.UserPrincipalName ?? driveItem.ParentReference.DriveId;
            item.Type = ItemTypes.OneDrive;
            item.FullPath = item.Name;
            return item;
        }

        public IItem CreateChild(DriveItem driveItem, IItem parent) {
            var item = IocContainer.Default.GetTransient<OneDriveItem>();
            item.Id = driveItem.Id;
            item.Name = driveItem.Name;
            item.Type = IsFolder(driveItem) ? ItemTypes.Folder :
                                ItemFactoryHelper.GetFileType(driveItem.Name);
            item.FullPath = Path.Combine(parent.FullPath, item.Name);
            return item;
        }


        private bool IsFolder(DriveItem driveItem) {
            return driveItem.Folder != null;
        }

    }
}
