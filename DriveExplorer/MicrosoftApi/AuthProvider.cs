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
using System.Configuration;
using System.Collections.Specialized;

namespace DriveExplorer.MicrosoftApi {
	public class AuthProvider : IAuthenticationProvider {
		/// <summary>
		/// Get default singleton of <see cref="AuthProvider"/> with default app configuration, which will automatically search for user secrets within this assembly.
		/// </summary>
		/// <exception cref="TypeInitializationException"> If there's no user secrets registered.</exception>
		public static AuthProvider Default { get; private set; } = new AuthProvider();

		private readonly IPublicClientApplication msalClient;
		private IAccount userAccount;
		private readonly string username;
		private readonly string password;

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
		public AuthProvider(string authority = Urls.Auth.Common) {
			var appConfig = ConfigurationManager.AppSettings;

			if (!ContainsKey(appConfig, nameof(appId))) {
				throw new ArgumentException($"Given {nameof(appConfig)} has no  configuration key named {nameof(appId)}");
			}
			appId = appConfig[nameof(appId)];
			if (ContainsKey(appConfig, nameof(Scopes))) {
				Scopes = appConfig[nameof(Scopes)].Split(';');
			}
			username = appConfig[nameof(username)];
			password = appConfig[nameof(password)];
			msalClient = PublicClientApplicationBuilder
				.Create(appId)
				.WithAuthority(authority)
				.WithDefaultRedirectUri()
				.Build();

			bool ContainsKey(NameValueCollection appConfig, string key) {
				return appConfig.AllKeys.Any(item => item == key);
			}
		}

		/// <summary>
		/// Get access token silently, if not successful, then get interactively. If both failed, return null.
		/// </summary>
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
			try {
				// If there is an account, call AcquireTokenSilent
				using var cts = new CancellationTokenSource(Timeouts.Silent);
				// By doing this, MSAL will refresh the token automatically if
				// it is expired. Otherwise it returns the cached token.
				var result = await msalClient.AcquireTokenSilent(Scopes, userAccount)
											 .ExecuteAsync(cts.Token).ConfigureAwait(false); ;
				userAccount = result?.Account;
				return result?.AccessToken;
			} catch (MsalUiRequiredException) {
				return null;
			} catch (Exception ex) {
				Logger.ShowException(ex);
				return null;
			}
		}

		public async Task<string> GetAccessTokenInteractively() {
			using var cts = new CancellationTokenSource(Timeouts.Interactive);
			try {
				var result = await msalClient.AcquireTokenInteractive(Scopes)
										 .ExecuteAsync(cts.Token).ConfigureAwait(false);
				userAccount = result?.Account;
				return result?.AccessToken;
			} catch (Exception ex) {
				Logger.ShowException(ex);
				return null;
			}
		}

		/// <summary>
		/// This method can only be used with <see cref="Urls.Auth.Organizations"/> authority
		/// </summary>
		public async Task<string> GetAccessTokenWithUsernamePassword() {
			if (username == null || password == null) {
				return null;
			}
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var secureString = new SecureString();
			foreach (var c in password ?? "") {
				secureString.AppendChar(c);
			}
			try {
				var result = await msalClient
					.AcquireTokenByUsernamePassword(Scopes, username, secureString)
					.ExecuteAsync(cts.Token).ConfigureAwait(false); ;
				userAccount = result?.Account;
				return result?.AccessToken;
			} catch (Exception ex) {
				Logger.ShowException(ex);
				return null;
			}
		}
		/// <summary>
		/// Implementation of <see cref="IAuthenticationProvider"/>. This method is called everytime when user make a request.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public async Task AuthenticateRequestAsync(HttpRequestMessage request) {
			// attach authentication to the header of http request
			request.Headers.Authorization = new AuthenticationHeaderValue("bearer", await GetAccessTokenSilently().ConfigureAwait(false));
		}

	}
}