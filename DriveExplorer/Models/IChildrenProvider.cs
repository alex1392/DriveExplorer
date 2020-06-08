using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveExplorer.Models {
	public interface IChildrenProvider<IItem> {
		int Count();
		IList<IItem> Fetch(int startIndex, int pageSize);
	}
}
