﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DriveExplorer.Models {
	public class LocalItem : IItem {

		public ItemTypes Type { get; private set; }
		public string Name { get; private set; }
		public string FullPath { get; private set; }
		public long? Size { get; private set; }
		public DateTimeOffset? LastModifiedTime { get; private set; }

		/// <summary>
		/// Root constructor
		/// </summary>
		public LocalItem(string fullPath)
		{
			fullPath = fullPath;
			var info = new DirectoryInfo(fullPath);
			FullPath = fullPath;
			Type = ItemTypes.LocalDrive;
			Name = fullPath;
			Size = 0;
			LastModifiedTime = info.LastWriteTimeUtc;
		}


		/// <summary>
		/// Child constructor
		/// </summary>
		public LocalItem(string fullPath, bool isFolder)
		{
			fullPath = FixFullPath(fullPath);
			var info = isFolder ? new DirectoryInfo(fullPath) : new FileInfo(fullPath) as FileSystemInfo;
			FullPath = fullPath;
			Type = isFolder ? ItemTypes.Folder : ItemTypes.File;
			Name = Path.GetFileName(fullPath);
			Size = isFolder ? 0 : (info as FileInfo).Length;
			LastModifiedTime = info.LastWriteTimeUtc;
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

		private static string FixFullPath(string fullPath)
		{
			if (fullPath.Last() == Path.DirectorySeparatorChar) {
				fullPath = fullPath.Remove(fullPath.Length - 1);
			}

			return fullPath;
		}
		public Task DownloadAsync(string localPath)
		{
			// this should never be executed
			throw new InvalidOperationException();
		}
	}


}
