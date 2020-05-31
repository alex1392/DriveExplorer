using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveExplorer.ViewModels {
	public interface IDispatcher {
		public void Invoke(Action action);
	}
}
