using NUnit.Framework;

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DriveExplorer.MicrosoftApi.Tests {
	[TestFixtureSource(typeof(MicrosoftApiSource))]
	public class AuthProviderTests {
		private readonly AuthProvider authProvider;

		public AuthProviderTests(AuthProvider authProvider, GraphManager _) {
			this.authProvider = authProvider;
		}
		[Test()]
		public void AuthProviderTest() {
			Assert.NotNull(authProvider);
		}

		[Test()]
		public async Task GetAllAccessTokenSilentlyTestAsync() {
			var enumerable = authProvider.GetAllAccessTokenSilently();
			await foreach (var token in enumerable) {
				Console.WriteLine(token);
				Assert.NotNull(token);
			}
		}

		[Test()]
		public async Task GetAccessTokenSilentlyTestAsync() {
			var token = await authProvider.GetAccessTokenSilently().ConfigureAwait(false);
			Console.WriteLine(token);
			Assert.NotNull(token);
		}

		[Ignore("Required user interaction")]
		public async Task GetAccessTokenInteractivelyTestAsync() {
			var token = await authProvider.GetAccessTokenInteractively().ConfigureAwait(false);
			Console.WriteLine(token);
			Assert.NotNull(token);
		}

		[Test()]
		public async Task GetAccessTokenWithUsernamePasswordTestAsync() {
			var token = await authProvider.GetAccessTokenWithUsernamePassword().ConfigureAwait(false);
			Console.WriteLine(token);
			Assert.NotNull(token);
		}

		[Test()]
		public async Task AuthenticateRequestAsyncTestAsync() {
			var url = Path.Combine(GraphManager.ApiEndpoint, "me");
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			await authProvider.AuthenticateRequestAsync(request).ConfigureAwait(false);
			Console.WriteLine(request.Headers.Authorization);
			Assert.NotNull(request.Headers.Authorization);
		}

		[Test()]
		public void LogoutAsyncTest() {
			Console.WriteLine(authProvider.CurrentUserAccount);

			//authProvider.LogoutAsync()
		}
	}
}