using System.Collections.Generic;

namespace DriveExplorer {
    public interface IItem {
        ItemTypes Type { get; set; }
        string Name { get; set; }
        string FullPath { get; set; }
        IAsyncEnumerable<IItem> GetChildrenAsync();
    }
}
