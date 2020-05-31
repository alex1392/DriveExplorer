using Cyc.GoogleApi;
//using Cyc.MicrosoftApi;
using Cyc.Standard;
using DriveExplorer.Models;
using DriveExplorer.ViewModels;

using Google.Apis.Auth.OAuth2;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using System;
using System.Collections;
using System.IO;
using System.Linq;
//using System.Security.Permissions;


namespace DriveExplorer.Tests {
	//public static class DispatcherUtil {
	//	/// <summary>
	//	/// Make sure <see cref="Dispatcher.CurrentDispatcher"/> is working in unit test
	//	/// </summary>
	//	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	//	public static void DoEvents()
	//	{
	//		var frame = new DispatcherFrame();
	//		Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
	//			new DispatcherOperationCallback(ExitFrame), frame);
	//		Dispatcher.PushFrame(frame);
	//	}

	//	private static object ExitFrame(object frame)
	//	{
	//		((DispatcherFrame)frame).Continue = false;
	//		return null;
	//	}
	//}
	public class TestSource : IEnumerable {
		//private static readonly MicrosoftApiManager microsoftManager;
		private static readonly GoogleApiManager googleManager;
		private static readonly MainWindowVM mainWindowVM;
		private static readonly string googleDriveUserId;
		private static readonly IAccount oneDriveAccount;

		static TestSource()
		{
			//var app = new Application(); // ensure main thread
			//DispatcherUtil.DoEvents();
			var services = new ServiceCollection();
			services.AddSingleton<ILogger, DebugLogger>();
			//services.AddSingleton(sp => new MicrosoftApiManager(sp.GetService<ILogger>(), MicrosoftApiManager.Authority.Organizations));
			var fullPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, @"GoogleApi\client_secret.json");
			services.AddSingleton(sp =>
				new GoogleApiManager(sp.GetService<ILogger>(),
					fullPath,
					dataStorePath: Path.Combine(GoogleWebAuthorizationBroker.Folder, "Test")));

			services.AddSingleton<GoogleDriveManager>();
			services.AddSingleton<OneDriveManager>();
			services.AddSingleton<LocalDriveManager>();
			services.AddSingleton<MainWindowVM>();


			var serviceProvider = services.BuildServiceProvider();
			//microsoftManager = serviceProvider.GetService<MicrosoftApiManager>();
			googleManager = serviceProvider.GetService<GoogleApiManager>();
			mainWindowVM = serviceProvider.GetService<MainWindowVM>();
			mainWindowVM.SetCacheRootPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(DriveExplorer)));
			//var result = microsoftManager.LoginWithUsernamePassword().Result;
			//oneDriveAccount = result.Account;

			googleDriveUserId = googleManager.LoadAllUserId().First();
			googleManager.UserLoginAsync(googleDriveUserId).Wait();
		}
		public IEnumerator GetEnumerator()
		{
			yield return new object[] { new object[] {/* microsoftManager, */oneDriveAccount, googleManager, mainWindowVM, googleDriveUserId } };
		}
	}

	public class source : IEnumerable {
		public IEnumerator GetEnumerator()
		{
			yield return new object[] { 1 };
		}
	}
}