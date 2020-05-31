using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DriveExplorer.Models {
	public interface IItem {
		ItemTypes Type { get; }
		string Name { get; }
		string FullPath { get; }
		long? Size { get; }
		DateTimeOffset? LastModifiedTime { get; }
		Task DownloadAsync(string localPath);
		IAsyncEnumerable<IItem> GetChildrenAsync();
	}
}
