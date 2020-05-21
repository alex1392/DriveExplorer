using System.Collections.Generic;
using System.Linq;
using Path = System.IO.Path;

namespace DriveExplorer {
    public class ItemModel {
        public ItemModel(string fullPath) {
            FullPath = fullPath;
            var isRoot = IsRoot(fullPath);
            Type = isRoot ? Types.Drive :
                            IsFolder(fullPath) ? Types.Folder :
                                                 GetFileType(fullPath);
            if (isRoot) {
                Name = fullPath.Replace(Path.DirectorySeparatorChar, ' ').Trim();
            } else {
                if (fullPath.Last() == '\\') {
                    fullPath = fullPath.Remove(fullPath.Length - 1);
                }
                Name = Path.GetFileName(fullPath);
            }

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

        private static readonly Dictionary<string, Types> TypesMap = new Dictionary<string, Types>
        {
            {".jpg" , Types.IMG},
            {".jpeg", Types.IMG},
            {".png" , Types.IMG},
            {".bmp" , Types.IMG},
            {".gif" , Types.IMG},
            {".txt" , Types.TXT},
            {".doc" , Types.DOC},
            {".docx", Types.DOC},
            {".xls" , Types.XLS},
            {".xlsx", Types.XLS},
            {".ppt" , Types.PPT},
            {".pptx", Types.PPT},
            {".rar" , Types.ZIP},
            {".zip" , Types.ZIP},
        };
        private ItemModel() {

        }
        private Types GetFileType(string fullPath) {
            var ext = Path.GetExtension(fullPath).ToLower();
            if (TypesMap.ContainsKey(ext)) {
                return TypesMap[ext];
            }
            return Types.File;
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
