using DataVirtualization;
using DriveExplorer.Models;
using System.Collections.Generic;
using System.Linq;

namespace DriveExplorer.ViewModels {
	public class ItemVMChildrenProvider : IItemsProvider<ItemVM> {
		private readonly ItemVM itemVM;
		private readonly IItem item;

		public ItemVMChildrenProvider(ItemVM itemVM)
		{
			this.itemVM = itemVM;
			item = itemVM.Item;
		}

		public int FetchCount()
		{
			return item.ChildrenProvider.FetchCount();
		}

		public IList<ItemVM> FetchRange(int startIndex, int count)
		{
			var items = item.ChildrenProvider.FetchRange(startIndex, count);
			return items.Select(item => new ItemVM(item, itemVM)).ToList();
		}
	}
}