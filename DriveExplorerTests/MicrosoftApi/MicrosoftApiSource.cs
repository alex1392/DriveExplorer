using DriveExplorer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Collections;

namespace DriveExplorer.MicrosoftApi.Tests {
	public class MicrosoftApiSource : IEnumerable {
		private static readonly GraphManager graphManager;
		private static readonly MainWindowVM mainWindowVM;
		private static readonly IAccount account;

		static MicrosoftApiSource() {
			var services = new ServiceCollection();
			services.AddSingleton<ILogger, DebugLogger>();
			services.AddSingleton(sp => new GraphManager(sp.GetService<ILogger>(), GraphManager.Authority.Organizations));
			services.AddSingleton<MainWindowVM>();

			var serviceProvider = services.BuildServiceProvider();
			graphManager = serviceProvider.GetService<GraphManager>();
			mainWindowVM = serviceProvider.GetService<MainWindowVM>();
			(_, account) = graphManager.GetAccessTokenWithUsernamePassword().Result;		
		}
		public IEnumerator GetEnumerator() {
			yield return new object[] { new object[] { graphManager, account, mainWindowVM } };
		}
	}
}