using System;
using System.Collections.Generic;
using System.Linq;
using Directory = System.IO.Directory;
using System.Threading.Tasks;
using DriveExplorer.IoC;
using System.Windows;

namespace DriveExplorer {

    public class LocalItem : IItem {
        public ItemTypes Type { get; set; }
        public string Name { get; set; }
        public string FullPath { get; set; }

        public async IAsyncEnumerable<IItem> GetChildrenAsync() {
            IEnumerable<string> paths;
            try {
                paths = Directory.GetFiles(FullPath).Concat(Directory.GetDirectories(FullPath));
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show(ex.Message);
                yield break;
            }
            foreach (var path in paths) {
                yield return IocContainer.Default.GetSingleton<LocalItemFactory>().Create(path);
            }
        }

        public async IAsyncEnumerable<IItem> GetFilesAsync() {
            string[] filePaths;
            try {
                filePaths = Directory.GetFiles(FullPath);
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show(ex.Message);
                yield break;
            }
            foreach (var filePath in filePaths) {
                yield return IocContainer.Default.GetSingleton<LocalItemFactory>().Create(filePath);
            }
        }

        public async IAsyncEnumerable<IItem> GetFoldersAsync() {
            string[] folderPaths;
            try {
                folderPaths = Directory.GetDirectories(FullPath);
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show(ex.Message);
                yield break;
            }
            foreach (var folderPath in folderPaths) {
                yield return IocContainer.Default.GetSingleton<LocalItemFactory>().Create(folderPath);
            }
        }
    }

    public enum ItemTypes {
        Folder = 0b000000001,
        Drive  = 0b000000010,
        File   = 0b000000100,
        IMG    = 0b000001000,
        TXT    = 0b000010000,
        DOC    = 0b000100000,
        XLS    = 0b001000000,
        PPT    = 0b010000000,
        ZIP    = 0b100000000,
    }
    public static class ItemTypesExtensions {
        public static ItemTypes Add(this ItemTypes a, ItemTypes b) {
            return a | b;
        }
        public static ItemTypes Remove(this ItemTypes a, ItemTypes b) {
            return a & ~b;
        }
        public static bool Contains(this ItemTypes a, ItemTypes b) {
            return (a & b) == b;
        }
    }
}
