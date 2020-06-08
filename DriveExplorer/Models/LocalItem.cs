using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DriveExplorer.Models {

	public class LocalItem : IItem {

		#region Public Properties

		public string FullPath { get; private set; }
		public DateTimeOffset? LastModifiedTime { get; private set; }
		public string Name { get; private set; }
		public long? Size { get; private set; }
		public ItemTypes ItemType { get; private set; }
		public DriveTypes DriveType { get; private set; } = DriveTypes.LocalDrive;

		public IChildrenProvider<IItem> ChildrenProvider { get; private set; }

		#endregion Public Properties

		#region Public Constructors

		/// <summary>
		/// Root constructor
		/// </summary>
		public LocalItem(string fullPath)
		{
			var info = new DirectoryInfo(fullPath);
			FullPath = fullPath;
			ItemType = ItemTypes.Drive;
			Name = FixFullPath(fullPath);
			Size = 0;
			LastModifiedTime = info.LastWriteTimeUtc;
			ChildrenProvider = new LocalChildrenProvider(this);
		}

		/// <summary>
		/// Child constructor
		/// </summary>
		public LocalItem(string fullPath, bool isFolder)
		{
			fullPath = FixFullPath(fullPath);
			var info = isFolder ? new DirectoryInfo(fullPath) : new FileInfo(fullPath) as FileSystemInfo;
			FullPath = fullPath;
			ItemType = isFolder ? ItemTypes.Folder : ItemTypes.File;
			Name = Path.GetFileName(fullPath);
			Size = isFolder ? 0 : (info as FileInfo).Length;
			LastModifiedTime = info.LastWriteTimeUtc;
			ChildrenProvider = new LocalChildrenProvider(this);
		}

		#endregion Public Constructors

		#region Public Methods

		public Task DownloadAsync(string localPath)
		{
			// this should never be executed
			// or simply just do nothing
			return Task.CompletedTask;
		}

		public async IAsyncEnumerable<IItem> GetChildrenAsync()
		{
			IEnumerable<string> files;
			IEnumerable<string> folders;
			try {
				folders = Directory.GetDirectories(FullPath);
				files = Directory.GetFiles(FullPath);
			} catch (UnauthorizedAccessException ex) {
				MessageBox.Show(ex.Message);
				yield break;
			} catch (IOException ex) {
				MessageBox.Show(ex.Message);
				yield break;
			}
			foreach (var path in folders) {
				yield return new LocalItem(path, true);
			}
			foreach (var path in files) {
				yield return new LocalItem(path, false);
			}
		}

		#endregion Public Methods

		#region Private Methods

		private static string FixFullPath(string fullPath)
		{
			while (fullPath.Last() == Path.DirectorySeparatorChar) {
				fullPath = fullPath.Remove(fullPath.Length - 1);
			}

			return fullPath;
		}

		#endregion Private Methods
	}
}