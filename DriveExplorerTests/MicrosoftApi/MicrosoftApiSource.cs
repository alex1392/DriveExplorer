using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Collections;

namespace DriveExplorer.MicrosoftApi.Tests {
	public class MicrosoftApiSource : IEnumerable {
		private static readonly GraphManager graphManager;
		private static readonly IAccount account;

		static MicrosoftApiSource() {
			var services = new ServiceCollection();
			services.AddSingleton<ILogger, DebugLogger>();
			services.AddSingleton(sp => new GraphManager(sp.GetService<ILogger>(), GraphManager.Authority.Organizations));
			var serviceProvider = services.BuildServiceProvider();
			graphManager = serviceProvider.GetService<GraphManager>();
			(_, account) = graphManager.GetAccessTokenWithUsernamePassword().Result;
		}
		public IEnumerator GetEnumerator() {
			yield return new object[] { graphManager, account };
		}
	}
}