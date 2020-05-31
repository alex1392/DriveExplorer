using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DriveExplorer.ViewModels {
	public class ApplicationDispatcher : IDispatcher {

		public Dispatcher UnderlyingDispatcher {
			get {
				if (Application.Current == null || 
					Application.Current.Dispatcher == null) {
					throw new InvalidOperationException();
				}
				return Application.Current.Dispatcher;
			}
		}

		public void Invoke(Action action)
		{
			UnderlyingDispatcher.Invoke(action);
		}
	}
}
