using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace DriveExplorer.MicrosoftApi.Tests {
	public class MicrosoftApiSource : IEnumerable {
		private static readonly AuthProvider authProvider;
		private static readonly GraphManager graphManager;

		static MicrosoftApiSource() {
			var services = new ServiceCollection();
			services.AddSingleton<ILogger, DebugLogger>();
			services.AddSingleton(sp => new AuthProvider(sp.GetService<ILogger>(), AuthProvider.Authority.Organizations));
			services.AddSingleton<GraphManager>();
			var serviceProvider = services.BuildServiceProvider();
			authProvider = serviceProvider.GetService<AuthProvider>();
			graphManager = serviceProvider.GetService<GraphManager>();
			authProvider.GetAccessTokenWithUsernamePassword().Wait();
		}
		public IEnumerator GetEnumerator() {
			yield return new object[] { authProvider, graphManager };
		}
	}
}