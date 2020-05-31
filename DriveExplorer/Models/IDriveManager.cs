using System;
using System.Threading;
using System.Threading.Tasks;

namespace DriveExplorer.Models {
	public interface IDriveManager {
		event EventHandler<IItem> LoginCompleted;
		event EventHandler<IItem> LogoutCompleted;
		event EventHandler BeforeTaskExecuted;
		event EventHandler TaskExecuted;

		Task LoginAsync();
		Task AutoLoginAsync();
		Task LogoutAsync(IItem item);
		Task LoginAsync(CancellationToken token);
	}
}
