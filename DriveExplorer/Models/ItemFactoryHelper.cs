using System.Collections.Generic;

using Path = System.IO.Path;

namespace DriveExplorer.Models {
	public static class ItemFactoryHelper {
		private static readonly Dictionary<string, ItemTypes> FileTypesMap = new Dictionary<string, ItemTypes>
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

		public static ItemTypes GetFileType(string path) {
			var ext = Path.GetExtension(path).ToLower();
			if (FileTypesMap.ContainsKey(ext)) {
				return FileTypesMap[ext];
			}
			return ItemTypes.File;
		}

	}
}
