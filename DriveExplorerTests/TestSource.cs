using Cyc.GoogleApi;
using Cyc.MicrosoftApi;
using Cyc.Standard;
using DriveExplorer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

using System;
using System.Collections;
using System.IO;

namespace DriveExplorer.Tests {
	public class TestSource : IEnumerable {
		private static readonly MicrosoftManager microsoftManager;
		private static readonly GoogleManager googleManager;
		private static readonly MainWindowVM mainWindowVM;
		private static readonly IAccount account;

		static TestSource() {
			var services = new ServiceCollection();
			services.AddSingleton<ILogger, DebugLogger>();
			services.AddSingleton(sp => new MicrosoftManager(sp.GetService<ILogger>(), MicrosoftManager.Authority.Organizations));
			services.AddSingleton<MainWindowVM>();

			var fullPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, @"GoogleApi\client_secret.json");
			services.AddSingleton(sp => new GoogleManager(sp.GetService<ILogger>(), fullPath));

			var serviceProvider = services.BuildServiceProvider();
			microsoftManager = serviceProvider.GetService<MicrosoftManager>();
			googleManager = serviceProvider.GetService<GoogleManager>();
			mainWindowVM = serviceProvider.GetService<MainWindowVM>();
			var result = microsoftManager.LoginWithUsernamePassword().Result;
			account = result.Account;

			// TODO: login google accounts
		}
		public IEnumerator GetEnumerator() {
			yield return new object[] { new object[] { microsoftManager, account, googleManager, mainWindowVM } };
		}
	}
}