using System;

namespace DriveExplorer.ViewModels {

	public interface IDispatcher {

		#region Public Methods

		public void Invoke(Action action);

		#endregion Public Methods
	}
}