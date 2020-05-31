using DriveExplorer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveExplorer.Tests {
	class MockDispatcher : IDispatcher {

		public void Invoke(Action action)
		{
			action.Invoke();
		}
	}
}
