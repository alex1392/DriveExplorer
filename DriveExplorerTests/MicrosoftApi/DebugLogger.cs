using System;
using System.Diagnostics;
using System.Text;


namespace DriveExplorer.MicrosoftApi.Tests {
	/// <summary>
	/// TODO: move this to test project
	/// </summary>
	public class DebugLogger : ILogger {
		public void Log(Exception ex) {
			var stringBuilder = new StringBuilder();
			do {
				stringBuilder.Append($"====={ex.GetType()}====={Environment.NewLine}{ex.Message}");
				ex = ex.InnerException;
			} while (ex != null);
			Debug.WriteLine(stringBuilder.ToString());
		}
	}
}