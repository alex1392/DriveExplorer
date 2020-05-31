using Cyc.GoogleApi;
using Cyc.MicrosoftApi;
using Cyc.Standard;

using DriveExplorer.Models;
using DriveExplorer.ViewModels;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace DriveExplorer.Tests {

	public class TestSource : IEnumerable {

		#region Private Fields

		private static readonly string googleDriveUserId;
		private static readonly GoogleApiManager googleManager;
		private static readonly MainWindowVM mainWindowVM;
		private static readonly MicrosoftApiManager microsoftManager;
		private static readonly IAccount oneDriveAccount;

		#endregion Private Fields

		#region Public Constructors

		static TestSource()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IDispatcher, MockDispatcher>();
			services.AddSingleton<ILogger, DebugLogger>();
			services.AddSingleton(sp => new MicrosoftApiManager(sp.GetService<ILogger>(), MicrosoftApiManager.Authority.Organizations));
			var fullPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, @"GoogleApi\client_secret.json");
			services.AddSingleton(sp =>
				new GoogleApiManager(sp.GetService<ILogger>(),
					fullPath,
					dataStorePath: Path.Combine(GoogleWebAuthorizationBroker.Folder, "Test")));

			services.AddSingleton<LocalDriveManager>();
			services.AddSingleton<GoogleDriveManager>();
			services.AddSingleton<OneDriveManager>();

			services.AddSingleton<MainWindowVM>();

			var serviceProvider = services.BuildServiceProvider();
			microsoftManager = serviceProvider.GetService<MicrosoftApiManager>();
			googleManager = serviceProvider.GetService<GoogleApiManager>();
			mainWindowVM = serviceProvider.GetService<MainWindowVM>();
			var result = microsoftManager.LoginWithUsernamePassword().Result;
			oneDriveAccount = result.Account;

			googleDriveUserId = googleManager.LoadAllUserId().FirstOrDefault();
			if (googleDriveUserId == null) {
				var store = new FileDataStore(googleManager.DataStorePath);
				var fixturePath = Path.Combine(store.FolderPath, "TokenFixtures");
				var filePaths = Directory.GetFiles(fixturePath);
				foreach (var filePath in filePaths) {
					File.Copy(filePath, Path.Combine(store.FolderPath, Path.GetFileName(filePath)));
				}
				googleDriveUserId = googleManager.LoadAllUserId().FirstOrDefault();
				if (googleDriveUserId == null) {
					throw new InvalidOperationException("No google account is cached.");
				}
			}
			googleManager.UserLoginAsync(googleDriveUserId).Wait();
		}

		#endregion Public Constructors

		#region Public Methods

		public IEnumerator GetEnumerator()
		{
			yield return new object[] { new object[] { microsoftManager, oneDriveAccount, googleManager, mainWindowVM, googleDriveUserId } };
		}

		#endregion Public Methods
	}
}