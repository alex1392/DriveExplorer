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

		public LocalItem(string fullPath) {
			FullPath = fullPath;
			var isRoot = IsRoot(fullPath);
			Type = isRoot ? ItemTypes.LocalDrive :
							IsFolder(fullPath) ? ItemTypes.Folder :
												 ItemFactoryHelper.GetFileType(fullPath);
			if (isRoot) {
				Name = fullPath.Replace(Path.DirectorySeparatorChar, ' ').Trim();
			} else {
				if (fullPath.Last() == Path.DirectorySeparatorChar) {
					fullPath = fullPath.Remove(fullPath.Length - 1);
				}
				Name = Path.GetFileName(fullPath);
			}
		}
		private static bool IsRoot(string path) {
			while (path.Last() == Path.DirectorySeparatorChar) {
				path = path.Remove(path.Length - 1);
			}
			return path.Last() == ':';
		}

		private static bool IsFolder(string fullPath) {
			var ext = Path.GetExtension(fullPath);
			return string.IsNullOrEmpty(ext);
		}

		public async IAsyncEnumerable<IItem> GetChildrenAsync() {
			IEnumerable<string> paths;
			try {
				paths = Directory.GetFiles(FullPath).Concat(Directory.GetDirectories(FullPath));
			} catch (UnauthorizedAccessException ex) {
				MessageBox.Show(ex.Message);
				yield break;
			} catch (IOException ex) {
				MessageBox.Show(ex.Message);
				yield break;
			}
			foreach (var path in paths) {
				yield return new LocalItem(path);
			}
		}
	}

}
