using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DriveExplorer.MicrosoftApi {
	public class AuthProvider : IAuthenticationProvider {
		/// <summary>
		/// Get default singleton of <see cref="AuthProvider"/> with default app configuration, which will automatically search for user secrets within this assembly.
		/// </summary>
		/// <exception cref="TypeInitializationException"> If there's no user secrets registered.</exception>
		public static AuthProvider Default { get; private set; } = new AuthProvider();

		private readonly IPublicClientApplication msalClient;
		private readonly IConfigurationRoot appConfig;
		private IAccount userAccount;

		/// <summary>
		/// Specifies the scopes of graph api would be used in the current application.
		/// </summary>
		public string[] Scopes { get; set; }

		private readonly string appId;

		/// <summary>
		/// Get <see cref="AuthProvider"/> with <see cref="IConfigurationRoot"/>
		/// </summary>
		/// <param name="appConfig">Must contain settings named <see cref="appId"/>.</param>
		/// <exception cref="InvalidOperationException">Throws when getting default <see cref="AuthProvider"/> and if there's no user secrets registered.</exception>
		/// <exception cref="ArgumentException"/>
		public AuthProvider(IConfigurationRoot appConfig = null, string authority = Urls.Auth.Common) {
			if (appConfig == null) {
				appConfig = new ConfigurationBuilder()
					.AddUserSecrets<AuthProvider>() // may throws invalid operation exception
					.Build();
			}
			this.appConfig = appConfig;
			if (!ContainsKey(appConfig, nameof(appId))) {
				throw new ArgumentException($"Given {nameof(appConfig)} has no  configuration key named {nameof(appId)}");
			}
			appId = appConfig[nameof(appId)];
			if (ContainsKey(appConfig, nameof(Scopes))) {
				Scopes = appConfig[nameof(Scopes)].Split(';');
			}
			msalClient = PublicClientApplicationBuilder
				.Create(appId)
				.WithAuthority(AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
				.WithRedirectUri(Urls.LocalHost)
				.WithAuthority(authority)
				.Build();
			bool ContainsKey(IConfigurationRoot appConfig, string key) {
				return appConfig.GetChildren().Any(item => item.Key == key);
			}
		}


		public async Task<string> GetAccessToken() {
			return await GetAccessTokenSilently().ConfigureAwait(false) ??
				await GetAccessTokenInteractively().ConfigureAwait(false);
		}

		public async Task<string> GetAccessTokenSilently() {
			// TODO: why no cache for everytime the application restarted ???
			userAccount = (await msalClient.GetAccountsAsync().ConfigureAwait(false)).FirstOrDefault();
			if (userAccount == null) {
				return null;
			}
			// If there is an account, call AcquireTokenSilent
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			// By doing this, MSAL will refresh the token automatically if
			// it is expired. Otherwise it returns the cached token.
			var result = await msalClient.AcquireTokenSilent(Scopes, userAccount)
										 .ExecuteAsync(cts.Token).ConfigureAwait(false); ;
			userAccount = result?.Account;
			return result?.AccessToken;
		}

		public async Task<string> GetAccessTokenInteractively() {
			using var cts = new CancellationTokenSource(Timeouts.Interactive);
			var result = await msalClient.AcquireTokenInteractive(Scopes)
										 .ExecuteAsync(cts.Token).ConfigureAwait(false);
			userAccount = result?.Account;
			return result?.AccessToken;
		}

		/// <summary>
		/// This method can only be used with <see cref="Urls.Auth.Organizations"/> authority
		/// </summary>
		public async Task<string> GetAccessTokenWithUsernamePassword() {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			string Username = appConfig[nameof(Username)];
			string Password = appConfig[nameof(Password)];
			var secureString = new SecureString();
			foreach (var c in Password ?? "") {
				secureString.AppendChar(c);
			}
			var result = await msalClient
				.AcquireTokenByUsernamePassword(Scopes, Username, secureString)
				.ExecuteAsync(cts.Token).ConfigureAwait(false); ;
			userAccount = result?.Account;
			return result?.AccessToken;
		}

		public async Task<string> GetAccessTokenWithDeviceCode() {
			using var cts = new CancellationTokenSource(Timeouts.Interactive);
			// Invoke device code flow so user can sign-in with a browser
			var result = await msalClient.AcquireTokenWithDeviceCode(Scopes, deviceCodeCallback =>
			{
					// display instructions to let user follow the device code flow
					Console.WriteLine(deviceCodeCallback.Message);
					// display instructions in testing output as well
					Trace.WriteLine(deviceCodeCallback.Message);
				return Task.FromResult(0);
			}).ExecuteAsync(cts.Token).ConfigureAwait(false); ;
			userAccount = result?.Account;
			return result?.AccessToken;
		}


		/// <summary>
		/// Implementation of <see cref="IAuthenticationProvider"/>. This method is called everytime when user make a request.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public async Task AuthenticateRequestAsync(HttpRequestMessage request) {
			// attach authentication to the header of http request
			request.Headers.Authorization = new AuthenticationHeaderValue("bearer", await GetAccessToken().ConfigureAwait(false));
		}

	}
}