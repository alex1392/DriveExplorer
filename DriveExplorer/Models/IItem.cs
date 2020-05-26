using System.Collections.Generic;

namespace DriveExplorer.Models {
	public interface IItem {
		ItemTypes Type { get; }
		string Name { get; }
		string FullPath { get; }
		IAsyncEnumerable<IItem> GetChildrenAsync();
	}
}
