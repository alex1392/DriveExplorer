﻿using System;

namespace DriveExplorer.Models {
	[Flags]
	public enum ItemTypes {
		Unknown = 0,
		LocalDrive = 1,
		OneDrive = 1 << 1,
		GoogleDrive = 1 << 2,
		Folder = 1 << 3,
		File = 1 << 4,
		Drives = LocalDrive | OneDrive | GoogleDrive,
		Folders = Drives | Folder,
	}


	public static class ItemTypesExtensions {
		public static ItemTypes Add(this ItemTypes a, ItemTypes b)
		{
			return a | b;
		}
		public static ItemTypes Remove(this ItemTypes a, ItemTypes b)
		{
			return a & ~b;
		}
		public static bool Contains(this ItemTypes a, ItemTypes b)
		{
			return (a & b) == b;
		}
		public static bool Is(this ItemTypes a, ItemTypes b)
		{
			return (b & a) == a;
		}
	}
}
