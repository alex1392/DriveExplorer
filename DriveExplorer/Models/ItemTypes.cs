using System;

namespace DriveExplorer.Models {

	public static class ItemTypesExtensions {

		#region Public Methods

		public static ItemTypes Add(this ItemTypes a, ItemTypes b)
		{
			return a | b;
		}
		/// <summary>
		/// check if <paramref name="a"/> contains <paramref name="b"/>
		/// </summary>
		public static bool Contains(this ItemTypes a, ItemTypes b)
		{
			return (a & b) == b;
		}
		/// <summary>
		/// check if <paramref name="a"/> is member of <paramref name="b"/>
		/// </summary>
		public static bool IsMember(this ItemTypes a, ItemTypes b)
		{
			return (b & a) == a;
		}

		public static ItemTypes Remove(this ItemTypes a, ItemTypes b)
		{
			return a & ~b;
		}

		public static bool HaveAny(this ItemTypes a, ItemTypes b)
		{
			return (a & b) != 0;
		}

		#endregion Public Methods
	}

	[Flags]
	public enum ItemTypes {
		Unknown = 0,
		Folder = 1,
		File = 1 << 1,
		Drive = 1 << 2,
		Folders = Drive | Folder,
	}

	[Flags]
	public enum DriveTypes {
		Unknown = 0,
		LocalDrive = 1,
		OneDrive = 1 << 1,
		GoogleDrive = 1 << 2,
	}
}