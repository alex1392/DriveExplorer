using System;
using System.Collections.Generic;
using DriveExplorer.MicrosoftApi;
using DriveExplorer.IoC;

namespace DriveExplorer {
    public class OneDriveItem : IItem {
        private readonly GraphManager graphManager;

        public ItemTypes Type { get; set; }
        public string Name { get; set; }
        public string FullPath { get; set; }

        public string Id { get; set; }

        public OneDriveItem(GraphManager graphManager) {
            this.graphManager = graphManager;
        }

        public async IAsyncEnumerable<IItem> GetFilesAsync() {
            await foreach (var file in graphManager.GetFilesAsync(Id)) {
                yield return IocContainer.Default.GetSingleton<OneDriveItemFactory>().Create(file);
            }
        }

        public async IAsyncEnumerable<IItem> GetFoldersAsync() {
            await foreach (var folder in graphManager.GetFolersAsync(Id)) {
                yield return IocContainer.Default.GetSingleton<OneDriveItemFactory>().Create(folder);
            }
        }

        public async IAsyncEnumerable<IItem> GetChildrenAsync() {
            await foreach (var item in graphManager.GetChildrenAsync(Id)) {
                yield return IocContainer.Default.GetSingleton<OneDriveItemFactory>().Create(item);
            }
        }
    }
}
