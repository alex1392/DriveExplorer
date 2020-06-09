using DataVirtualization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DriveExplorer.Models {

	public interface IItem {

		#region Public Properties
		IItemsProvider<IItem> ChildrenProvider { get; }
		string FullPath { get; }
		DateTimeOffset? LastModifiedTime { get; }
		string Name { get; }
		long? Size { get; }
		ItemTypes ItemType { get; }
		DriveTypes DriveType { get; }

		#endregion Public Properties

		#region Public Methods

		Task DownloadAsync(string localPath);

		IAsyncEnumerable<IItem> GetChildrenAsync();
		IAsyncEnumerable<IItem> GetSubFolderAsync();

		#endregion Public Methods
	}
}