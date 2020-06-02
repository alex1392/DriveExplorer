using Cyc.Standard;

using System;
using System.IO;
using System.Threading.Tasks;

namespace DriveExplorer.Models {

	public class LocalDriveManager {

		#region Private Fields

		private readonly ILogger logger;

		#endregion Private Fields

		#region Public Events

		public event EventHandler<IItem> GetDriveCompleted;

		#endregion Public Events

		#region Public Constructors

		public LocalDriveManager(ILogger logger)
		{
			this.logger = logger;
		}

		#endregion Public Constructors

		#region Public Methods

		public void GetLocalDrives()
		{
			string[] drivePaths = null;
			try {
				drivePaths = Directory.GetLogicalDrives();
			} catch (UnauthorizedAccessException ex) {
				logger?.Log(ex);
			}
			foreach (var drivePath in drivePaths) {
				var item = new LocalItem(drivePath);
				GetDriveCompleted?.Invoke(this, item);
			}
		}

		public string GetDesktop()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		}

		#endregion Public Methods
	}
}