using Cyc.Standard;
using Syroot.Windows.IO;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DriveExplorer.Models {

	public class LocalDriveManager {

		#region Private Fields

		private readonly ILogger logger;

		#endregion Private Fields

		#region Public Events

		public event EventHandler<IItem> GetFolderCompleted;

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
				GetFolderCompleted?.Invoke(this, item);
			}
		}

		public void GetUserFolders()
		{
			var folderPaths = new[]
			{
				RecentPath, DesktopPath, DownloadsPath, DocumentsPath, PicturesPath, MusicPath, VideosPath,
			};
			foreach (var path in folderPaths) {
				var item = new LocalItem(path, ItemTypes.Folder);
				GetFolderCompleted?.Invoke(this, item);
			}
		}

		public string RecentPath => KnownFolders.Recent.Path;
		public string DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		public string DownloadsPath => KnownFolders.Downloads.Path;
		public string DocumentsPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		public string PicturesPath => KnownFolders.Pictures.Path;
		public string VideosPath => KnownFolders.Videos.Path;
		public string MusicPath => KnownFolders.Music.Path;


		#endregion Public Methods
	}
}