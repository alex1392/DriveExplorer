using System;
using System.Collections.Generic;

namespace DriveExplorer.Models {
	public interface IItem {
		ItemTypes Type { get; }
		string Name { get; }
		string FullPath { get; }
		long? Size { get; }
		DateTimeOffset? LastModifiedTime { get; }
		IAsyncEnumerable<IItem> GetChildrenAsync();
	}
}
