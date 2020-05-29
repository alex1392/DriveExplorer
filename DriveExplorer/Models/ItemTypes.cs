using System;

namespace DriveExplorer.Models {
	[Flags]
	public enum ItemTypes {
		Unknown = 0,
		Folder = 1,
		LocalDrive = 1 << 1,
		OneDrive = 1 << 2,
		File = 1 << 3,
		IMG = 1 << 4,
		TXT = 1 << 5,
		DOC = 1 << 6,
		XLS = 1 << 7,
		PPT = 1 << 8,
		ZIP = 1 << 9,
		GoogleDrive = 1 << 10,
		Folders = Folder | Drives,
		Drives = OneDrive | LocalDrive | GoogleDrive,
		Files = File | IMG | TXT | DOC | XLS | PPT | ZIP,
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
		public static bool Is(this ItemTypes a, ItemTypes b) {
			return (b & a) == a;
		}
	}
}
