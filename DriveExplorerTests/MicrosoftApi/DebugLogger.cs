using System;
using System.Text;


namespace DriveExplorer.MicrosoftApi.Tests {

	public class DebugLogger : ILogger {
		public void Log(Exception ex) {
			var stringBuilder = new StringBuilder();
			do {
				stringBuilder.Append($"====={ex.GetType()}====={Environment.NewLine}{ex.Message}");
				ex = ex.InnerException;
			} while (ex != null);
			Console.WriteLine(stringBuilder.ToString());
		}

		public void Log(string message) {
			Console.WriteLine(message);
		}
	}
}