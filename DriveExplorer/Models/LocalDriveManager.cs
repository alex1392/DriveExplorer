using Cyc.Standard;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DriveExplorer.Models {
	public class LocalDriveManager {
		private readonly ILogger logger;

		public event EventHandler<IItem> LoginCompleted;

		public LocalDriveManager(ILogger logger)
		{
			this.logger = logger;
		}
		public Task AutoLoginAsync()
		{
			string[] drivePaths = null;
			try {
				drivePaths = Directory.GetLogicalDrives();
			} catch (UnauthorizedAccessException ex) {
				logger?.Log(ex);
			}
			foreach (var drivePath in drivePaths) {
				var item = new LocalItem(drivePath);
				LoginCompleted?.Invoke(this, item);
			}
			return Task.CompletedTask;
		}
	}
}
