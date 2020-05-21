using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;
using System;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Security;

namespace DriveExplorer.MicrosoftApi {
	public class AuthProvider : IAuthenticationProvider {
		private static AuthProvider instance;
		public static AuthProvider Instance => instance;
		// lazy singleton with arguments
		public static void Initialize(IConfigurationRoot appConfig) {
			if (instance == null) {
				instance = new AuthProvider(appConfig);
			} else {
				Console.WriteLine($"{typeof(AuthProvider)} has already been initialized.");
			}
		}

		private readonly IPublicClientApplication msalClient;
		private readonly IConfigurationRoot appConfig;
		private string[] scopes;
		private IAccount userAccount;

		public string[] Scopes {
			get => scopes;
			set => scopes = value;
		}

		private AuthProvider(IConfigurationRoot appConfig) {
			this.appConfig = appConfig;
			scopes = appConfig[nameof(scopes)].Split(';');
			string appId = appConfig[nameof(appId)];
			msalClient = PublicClientApplicationBuilder
				.Create(appId)
				.WithAuthority(AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount, true)
				// for interactively sign in
				.WithRedirectUri("http://localhost")
				// for username & password sign in
				.WithAuthority("https://login.microsoftonline.com/organizations")
				.Build();
			userAccount = msalClient.GetAccountsAsync().Result.FirstOrDefault();
		}


		public async Task<string> GetAccessToken(
			bool GetSilently = true,
			bool GetWithUsernamePassword = true,
			bool GetInteractively = true,
			bool GetWithDeviceCode = true) {
			string result = null;
			// check if user already signed in
			if (GetSilently && userAccount != null) {
				result = await GetAccessTokenSilently();
			}
			if (GetWithUsernamePassword && result == null) {
				result = await GetAccessTokenWithUsernamePassword();
			}
			if (GetInteractively && result == null) {
				result = await GetAccessTokenInteractively();
			}
			if (GetWithDeviceCode && result == null) {
				result = await GetAccessTokenWithDeviceCode();
			}

			return result;
		}

		public async Task<string> GetAccessTokenSilently() {
			// If there is an account, call AcquireTokenSilent
			// By doing this, MSAL will refresh the token automatically if
			// it is expired. Otherwise it returns the cached token.
			using (var cts = new CancellationTokenSource(Timeouts.Silent)) {
				var result = await msalClient
					.AcquireTokenSilent(scopes, userAccount)
					.ExecuteAsync(cts.Token);
				userAccount = result?.Account;
				return result?.AccessToken;
			}
		}

		// Not applicable for personal account
		public async Task<string> GetAccessTokenWithUsernamePassword() {
			using (var cts = new CancellationTokenSource(Timeouts.Silent)) {
				string Username = appConfig[nameof(Username)];
				string Password = appConfig[nameof(Password)];
				var secureString = new SecureString();
				foreach (var c in Password) {
					secureString.AppendChar(c);
				}
				var result = await msalClient
					.AcquireTokenByUsernamePassword(scopes, Username, secureString)
					.ExecuteAsync(cts.Token);
				userAccount = result?.Account;
				return result?.AccessToken;
			}
		}

		public async Task<string> GetAccessTokenWithDeviceCode() {
			using (var cts = new CancellationTokenSource(Timeouts.Interactive)) {
				// Invoke device code flow so user can sign-in with a browser
				var result = await msalClient.AcquireTokenWithDeviceCode(scopes, deviceCodeCallback => {
					// display instructions to let user follow the device code flow
					Console.WriteLine(deviceCodeCallback.Message);
					// display instructions in testing output as well
					Trace.WriteLine(deviceCodeCallback.Message);
					return Task.FromResult(0);
				}).ExecuteAsync(cts.Token);
				userAccount = result?.Account;
				return result?.AccessToken;
			}
		}

		public async Task<string> GetAccessTokenInteractively() {
			using (var cts = new CancellationTokenSource(Timeouts.Interactive)) {
				var result = await msalClient
					.AcquireTokenInteractive(scopes)
					.ExecuteAsync(cts.Token);
				userAccount = result?.Account;
				return result?.AccessToken;
			}
		}

		/// <summary>
		/// Implementation of <see cref="IAuthenticationProvider"/>. This method is called everytime when user make a request.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public async Task AuthenticateRequestAsync(HttpRequestMessage request) {
			// attach authentication to the header of http request
			request.Headers.Authorization = new AuthenticationHeaderValue("bearer", await GetAccessToken());
		}

	}
}