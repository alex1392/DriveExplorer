﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DriveExplorer.Models {

	public interface IItem {

		#region Public Properties

		string FullPath { get; }
		DateTimeOffset? LastModifiedTime { get; }
		string Name { get; }
		long? Size { get; }
		ItemTypes Type { get; }

		#endregion Public Properties

		#region Public Methods

		Task DownloadAsync(string localPath);

		IAsyncEnumerable<IItem> GetChildrenAsync();

		#endregion Public Methods
	}
}