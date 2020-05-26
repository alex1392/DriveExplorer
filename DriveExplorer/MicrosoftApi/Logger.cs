using System;
using System.Text;
using System.Windows;


namespace DriveExplorer.MicrosoftApi {
	public static class Logger {
		public static void ShowException(Exception ex) {
			var stringBuilder = new StringBuilder();
			do {
				stringBuilder.Append($"====={ex.GetType()}====={Environment.NewLine}{ex.Message}");
				ex = ex.InnerException;
			} while (ex != null);
			MessageBox.Show(stringBuilder.ToString());
		}
	}

}