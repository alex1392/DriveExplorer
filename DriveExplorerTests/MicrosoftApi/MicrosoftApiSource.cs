using DriveExplorer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Collections;

namespace DriveExplorer.MicrosoftApi.Tests {
	public class MicrosoftApiSource : IEnumerable {
		private static readonly MicrosoftManager graphManager;
		private static readonly MainWindowVM mainWindowVM;
		private static readonly IAccount account;

		static MicrosoftApiSource() {
			var services = new ServiceCollection();
			services.AddSingleton<ILogger, DebugLogger>();
			services.AddSingleton(sp => new MicrosoftManager(sp.GetService<ILogger>(), MicrosoftManager.Authority.Organizations));
			services.AddSingleton<MainWindowVM>();

			var serviceProvider = services.BuildServiceProvider();
			graphManager = serviceProvider.GetService<MicrosoftManager>();
			mainWindowVM = serviceProvider.GetService<MainWindowVM>();
			(_, account) = graphManager.GetAccessTokenWithUsernamePassword().Result;		
		}
		public IEnumerator GetEnumerator() {
			yield return new object[] { new object[] { graphManager, account, mainWindowVM } };
		}
	}
}