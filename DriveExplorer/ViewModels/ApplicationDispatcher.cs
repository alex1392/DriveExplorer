using System;
using System.Windows;
using System.Windows.Threading;

namespace DriveExplorer.ViewModels {

	public class ApplicationDispatcher : IDispatcher {

		#region Public Properties

		public Dispatcher UnderlyingDispatcher {
			get {
				if (Application.Current == null ||
					Application.Current.Dispatcher == null) {
					throw new InvalidOperationException();
				}
				return Application.Current.Dispatcher;
			}
		}

		#endregion Public Properties

		#region Public Methods

		public void Invoke(Action action)
		{
			UnderlyingDispatcher.Invoke(action);
		}

		#endregion Public Methods
	}
}