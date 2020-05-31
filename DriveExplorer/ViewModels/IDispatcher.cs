using System;

namespace DriveExplorer.ViewModels {
	public interface IDispatcher {
		public void Invoke(Action action);
	}
}
