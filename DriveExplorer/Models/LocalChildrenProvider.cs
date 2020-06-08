﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DriveExplorer.Models {
	public class LocalChildrenProvider : IChildrenProvider<IItem> {
		private LocalItem localItem;
		private readonly string[] filePaths;
		private readonly string[] folderPaths;

		public LocalChildrenProvider(LocalItem parent)
		{
			this.localItem = parent;
			filePaths = Directory.GetFiles(parent.FullPath);
			folderPaths = Directory.GetDirectories(parent.FullPath);
		}

		public int Count()
		{
			return filePaths.Length + folderPaths.Length;
		}

		public IList<IItem> Fetch(int startIndex, int pageSize)
		{
			startIndex = Math.Max(0, startIndex);
			return folderPaths.Skip(startIndex)
			   .Take(pageSize)
			   .Select(path => new LocalItem(path, true))
			   .Concat(filePaths.Skip(Math.Max(startIndex - folderPaths.Length, 0))
					   .Take(pageSize - Math.Max(folderPaths.Length - startIndex, 0))
					   .Select(path => new LocalItem(path, false)))
			   .Cast<IItem>()
			   .ToList();
		}
	}
}