using System;
using System.Threading;
using System.Threading.Tasks;

namespace DriveExplorer.Models {

	public interface IDriveManager {

		#region Public Events

		event EventHandler BeforeTaskExecuted;

		event EventHandler<IItem> LoginCompleted;

		event EventHandler<IItem> LogoutCompleted;

		event EventHandler TaskExecuted;

		#endregion Public Events

		#region Public Methods

		Task AutoLoginAsync();

		Task LoginAsync();

		Task LoginAsync(CancellationToken token);

		Task LogoutAsync(IItem item);

		#endregion Public Methods
	}
}