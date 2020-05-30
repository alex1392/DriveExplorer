using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Directory = System.IO.Directory;

namespace DriveExplorer.Models {
	public class LocalItem : IItem {
		public ItemTypes Type { get; private set; }
		public string Name { get; private set; }
		public string FullPath { get; private set; }
		public long Size { get; private set; }
		public DateTime LastModifiedDate { get; private set; }

		public LocalItem(string fullPath) {
			while (fullPath.Last() == Path.DirectorySeparatorChar) {
				fullPath = fullPath.Remove(fullPath.Length - 1);
			}
			FullPath = fullPath;
		}

		public static LocalItem CreateRoot(string fullPath) {
			var info = new DirectoryInfo(fullPath);
			var item = new LocalItem(fullPath)
			{
				Type = ItemTypes.LocalDrive,
				Name = fullPath,
				Size = 0,
				LastModifiedDate = info.LastWriteTimeUtc
			};
			return item;
		}
		public static LocalItem CreateFolder(string fullPath) {
			var info = new DirectoryInfo(fullPath);
			var item = new LocalItem(fullPath)
			{
				Type = ItemTypes.Folder,
				Name = Path.GetFileName(fullPath),
				Size = 0,
				LastModifiedDate = info.LastWriteTimeUtc
			};
			return item;
		}
		public static LocalItem CreateFile(string fullPath) {
			var info = new FileInfo(fullPath);
			var item = new LocalItem(fullPath)
			{
				Type = ItemFactoryHelper.GetFileType(fullPath),
				Name = Path.GetFileName(fullPath),
				Size = info.Length,
				LastModifiedDate = info.LastWriteTimeUtc
			};
			return item;
		}

		public async IAsyncEnumerable<IItem> GetChildrenAsync() {
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
				yield return CreateFolder(path);
			}
			foreach (var path in files) {
				yield return CreateFile(path);
			}
		}
	}

}
