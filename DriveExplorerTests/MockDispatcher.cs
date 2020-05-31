using DriveExplorer.ViewModels;

using System;

namespace DriveExplorer.Tests {

	internal class MockDispatcher : IDispatcher {

		#region Public Methods

		public void Invoke(Action action)
		{
			action.Invoke();
		}

		#endregion Public Methods
	}
}