﻿using Cyc.Standard;

using System;
using System.Text;
using System.Windows;

namespace DriveExplorer.Views {

	public class MessageBoxLogger : ILogger {

		#region Public Methods

		public void Log(Exception ex)
		{
			var stringBuilder = new StringBuilder();
			do {
				stringBuilder.Append($"====={ex.GetType()}====={Environment.NewLine}{ex.Message}");
				ex = ex.InnerException;
			} while (ex != null);
			MessageBox.Show(stringBuilder.ToString());
		}

		public void Log(string message)
		{
			MessageBox.Show(message);
		}

		#endregion Public Methods
	}
}