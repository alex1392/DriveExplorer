using System;
using Path = System.IO.Path;

namespace TreeViewTest {
    public class ItemModel {
        public ItemModel() {

        }
        public ItemModel(Types type, string fullPath, bool isRoot = false) {
            Type = type;
            FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
            Name = isRoot ? 
                fullPath.Replace(Path.DirectorySeparatorChar, ' ').Trim() :
                Path.GetFileName(fullPath);
        }

        public enum Types {
            Folder,
            Drive,
            File,
            IMG,
            TXT,
            DOC,
            XLS,
            PPT,
            ZIP,
        }
        public Types Type { get; set; }
        public string Name { get; set; }
        public string FullPath { get; set; }

    }

}
