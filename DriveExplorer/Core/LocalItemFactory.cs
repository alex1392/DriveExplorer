﻿using System.Linq;
using Path = System.IO.Path;

namespace DriveExplorer {
    public class LocalItemFactory {
        public IItem Create(string fullPath) {
            var item = new LocalItem
            {
                FullPath = fullPath
            };
            var isRoot = IsRoot(fullPath);
            item.Type = isRoot ? ItemTypes.Drive :
                            IsFolder(fullPath) ? ItemTypes.Folder :
                                                 ItemFactoryHelper.GetFileType(fullPath);
            if (isRoot) {
                item.Name = fullPath.Replace(Path.DirectorySeparatorChar, ' ').Trim();
            } else {
                if (fullPath.Last() == Path.DirectorySeparatorChar) {
                    fullPath = fullPath.Remove(fullPath.Length - 1);
                }
                item.Name = Path.GetFileName(fullPath);
            }
            return item;
        }
        private bool IsRoot(string fullPath) {
            var n = fullPath.Length;
            return fullPath[n - 1] == '\\' && fullPath[n - 2] == ':';
        }
        private bool IsFolder(string fullPath) {
            var ext = Path.GetExtension(fullPath);
            return string.IsNullOrEmpty(ext);
        }
    }
}
