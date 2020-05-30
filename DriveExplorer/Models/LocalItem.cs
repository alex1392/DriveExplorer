using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Directory = System.IO.Directory;
using FileSystemInfo = System.IO.FileSystemInfo;

namespace DriveExplorer.Models {
	public class LocalItem : IItem {
		private readonly Factory factory;
		public class Factory : ItemFactoryBase {
			public override IItem CreateRoot(params object[] parameters) {
				var fullPath = (string)parameters[0];
				while (fullPath.Last() == Path.DirectorySeparatorChar) {
					fullPath = fullPath.Remove(fullPath.Length - 1);
				}
				var info = new DirectoryInfo(fullPath);
				var item = new LocalItem(fullPath, this, info)
				{
					Type = ItemTypes.LocalDrive,
					Name = fullPath,
					Size = 0,
				};
				return item;
			}
			public override IItem CreateFolder(params object[] parameters) {
				var fullPath = (string)parameters[0];
				var info = new DirectoryInfo(fullPath);
				var item = new LocalItem(fullPath, this, info)
				{
					Type = ItemTypes.Folder,
					Name = Path.GetFileName(fullPath),
					Size = 0,
				};
				return item;
			}
			public override IItem CreateFile(params object[] parameters) {
				var fullPath = (string)parameters[0];
				var info = new FileInfo(fullPath);
				var item = new LocalItem(fullPath, this, info)
				{
					Type = GetFileType(fullPath),
					Name = Path.GetFileName(fullPath),
					Size = info.Length,
				};
				return item;
			}

		}

		public ItemTypes Type { get; private set; }
		public string Name { get; private set; }
		public string FullPath { get; private set; }
		public long? Size { get; private set; }
		public DateTimeOffset? LastModifiedTime { get; private set; }

		public LocalItem(string fullPath, Factory factory, FileSystemInfo info) {
			FullPath = fullPath;
			this.factory = factory;
			LastModifiedTime = info.LastWriteTimeUtc;
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
				yield return factory.CreateFolder(path);
			}
			foreach (var path in files) {
				yield return factory.CreateFile(path);
			}
		}

		public Task DownloadAsync(string localPath)
		{
			throw new NotImplementedException();
		}
	}


}
