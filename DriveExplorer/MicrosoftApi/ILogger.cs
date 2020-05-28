using System;


namespace DriveExplorer.MicrosoftApi {
	public interface ILogger {
		void Log(Exception ex);
		void Log(string message);
	}
}