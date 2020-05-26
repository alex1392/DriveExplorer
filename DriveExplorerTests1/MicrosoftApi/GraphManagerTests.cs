using NUnit.Framework;
using DriveExplorer.MicrosoftApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DriveExplorer.MicrosoftApi.Tests {
	[TestFixture()]
	public class GraphManagerTests {
		private AuthProvider authProvider;
		private GraphManager graphManager;

		[OneTimeSetUp]
		public void OneTimeSetup() {
			var services = new ServiceCollection();
			services.AddSingleton<ILogger, DebugLogger>();
			services.AddSingleton(sp => new AuthProvider(sp.GetService<ILogger>(), AuthProvider.Authority.Organizations));
			services.AddSingleton<GraphManager>();
			var serviceProvider = services.BuildServiceProvider();
			authProvider = serviceProvider.GetService<AuthProvider>();
			graphManager = serviceProvider.GetService<GraphManager>();
			authProvider.GetAccessTokenWithUsernamePassword().Wait();
		}

		[Test()]
		public void GraphManagerTest() {
			Assert.NotNull(graphManager);
		}

		[Test()]
		public async Task GetMeAsyncTestAsync() {
			var user = await graphManager.GetMeAsync().ConfigureAwait(false);
			Assert.NotNull(user);
		}

		[Test()]
		public void GetDriveRootAsyncTest() {
			throw new NotImplementedException();
		}

		[Test()]
		public void SearchDriveAsyncTest() {
			throw new NotImplementedException();
		}

		[Test()]
		public void GetFileAsyncTest() {
			throw new NotImplementedException();
		}

		[Test()]
		public void GetChildrenAsyncTest() {
			throw new NotImplementedException();
		}

		[Test()]
		public void UploadFileAsyncTest() {
			throw new NotImplementedException();
		}

		[Test()]
		public void UpdateFileAsyncTest() {
			throw new NotImplementedException();
		}
	}
}