using System.Collections.Generic;
using Path = System.IO.Path;

namespace DriveExplorer {
    public abstract class ItemFactoryHelper {
        static readonly Dictionary<string, ItemTypes> TypesMap = new Dictionary<string, ItemTypes>
        {
            {".jpg" , ItemTypes.IMG},
            {".jpeg", ItemTypes.IMG},
            {".png" , ItemTypes.IMG},
            {".bmp" , ItemTypes.IMG},
            {".gif" , ItemTypes.IMG},
            {".txt" , ItemTypes.TXT},
            {".doc" , ItemTypes.DOC},
            {".docx", ItemTypes.DOC},
            {".xls" , ItemTypes.XLS},
            {".xlsx", ItemTypes.XLS},
            {".ppt" , ItemTypes.PPT},
            {".pptx", ItemTypes.PPT},
            {".rar" , ItemTypes.ZIP},
            {".zip" , ItemTypes.ZIP},
        };

        public static ItemTypes GetFileType(string fullPath) {
            var ext = Path.GetExtension(fullPath).ToLower();
            if (TypesMap.ContainsKey(ext)) {
                return TypesMap[ext];
            }
            return ItemTypes.File;
        }

    }
}
