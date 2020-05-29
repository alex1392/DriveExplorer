using Cyc.GoogleApi;
using Cyc.MicrosoftApi;
using Cyc.Standard;

using DriveExplorer.ViewModels;

using Google.Apis.Auth.OAuth2;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace DriveExplorer.Tests {
	public class TestSource : IEnumerable {
		private static readonly MicrosoftManager microsoftManager;
		private static readonly GoogleManager googleManager;
		private static readonly MainWindowVM mainWindowVM;
		private static readonly string googleDriveUserId;
		private static readonly IAccount oneDriveAccount;

		static TestSource() {
			var services = new ServiceCollection();
			services.AddSingleton<ILogger, DebugLogger>();
			services.AddSingleton(sp => new MicrosoftManager(sp.GetService<ILogger>(), MicrosoftManager.Authority.Organizations));
			services.AddSingleton<MainWindowVM>();

			var fullPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, @"GoogleApi\client_secret.json");
			services.AddSingleton(sp =>
				new GoogleManager(sp.GetService<ILogger>(),
					fullPath,
					dataStorePath: Path.Combine(GoogleWebAuthorizationBroker.Folder, "Test")));

			var serviceProvider = services.BuildServiceProvider();
			microsoftManager = serviceProvider.GetService<MicrosoftManager>();
			googleManager = serviceProvider.GetService<GoogleManager>();
			mainWindowVM = serviceProvider.GetService<MainWindowVM>();
			var result = microsoftManager.LoginWithUsernamePassword().Result;
			oneDriveAccount = result.Account;

			googleDriveUserId = googleManager.LoadAllUserId().First();
			googleManager.UserLoginAsync(googleDriveUserId).Wait();
		}
		public IEnumerator GetEnumerator() {
			yield return new object[] { new object[] { microsoftManager, oneDriveAccount, googleManager, mainWindowVM, googleDriveUserId } };
		}
	}
}