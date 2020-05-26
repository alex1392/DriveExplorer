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
using System.Collections.Generic;
using System.IO;
using Prompt = Microsoft.Identity.Client.Prompt;


namespace DriveExplorer.MicrosoftApi {
	public class AuthProvider : IAuthenticationProvider {
		public static class Authority {
			public const string Organizations = "https://login.microsoftonline.com/organizations";
			public const string Comsumers = "https://login.microsoftonline.com/comsumers";
			public const string Common = "https://login.microsoftonline.com/common";
		}
		public static class RedirectUrl {
			public const string LocalHost = "http://localhost";
			public const string NativeClient = "https://login.microsoftonline.com/common/oauth2/nativeclient";
		}
		public static class Permissions {
			public static class User {
				public const string Read = "User.Read";
				public const string ReadAll = "User.Read.All";
				public const string ReadWrite = "User.ReadWrite";
				public const string ReadWriteAll = "User.ReadWrite.All";
			}

			public static class Files {
				public const string Read = "Files.Read";
				public const string ReadAll = "Files.Read.All";
				public const string ReadWrite = "Files.ReadWrite";
				public const string ReadWriteAll = "Files.ReadWrite.All";
			}
		}
		public static class Timeouts {
			public static readonly TimeSpan Silent = TimeSpan.FromSeconds(10);
			public static readonly TimeSpan Interactive = TimeSpan.FromMinutes(1);
		}

		private readonly IPublicClientApplication msalClient;
		private readonly string appId;
		private readonly string username;
		private readonly string password;

		/// <summary>
		/// Specifies the scopes of graph api would be used in the current application.
		/// </summary>
		public string[] Scopes { get; set; }
		public IAccount CurrentUserAccount { get; private set; }
		/// <summary>
		/// This property is used to find the corresponding <see cref="IAccount"/> from the given user id. They should be registered just after an user logged in.
		/// </summary>
		public Dictionary<string, IAccount> UserIdAccountRegistry { get; } = new Dictionary<string, IAccount>();

		public AuthProvider() : this(Authority.Common) {

		}

		/// <summary>
		/// Get <see cref="AuthProvider"/> with <see cref="IConfigurationRoot"/>
		/// </summary>
		/// <param name="appConfig">Must contain settings named <see cref="appId"/>.</param>
		/// <exception cref="InvalidOperationException">Throws when getting default <see cref="AuthProvider"/> and if there's no user secrets registered.</exception>
		/// <exception cref="ArgumentException"/>
		public AuthProvider(string authority) {
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
				.WithBroker(true)
				.WithAuthority(authority)
				.WithDefaultRedirectUri()
				.WithLogging(Log, LogLevel.Verbose, true)
				.Build();
			TokenCacheHelper.EnableSerialization(msalClient.UserTokenCache);

			bool ContainsKey(NameValueCollection appConfig, string key) {
				return appConfig.AllKeys.Any(item => item == key);
			}
		}

		private void Log(LogLevel level, string message, bool containsPii) {
			if (containsPii) {
				Console.ForegroundColor = ConsoleColor.Red;
			}
			Console.WriteLine($"{level} {message}");
			Console.ResetColor();
		}

		public async IAsyncEnumerable<string> GetAllAccessTokenSilently() {
			var users = await msalClient.GetAccountsAsync().ConfigureAwait(false);
			foreach (var user in users) {
				AuthenticationResult result;
				try {
					using var cts = new CancellationTokenSource(Timeouts.Silent);
					result = await msalClient.AcquireTokenSilent(Scopes, user)
												 .ExecuteAsync(cts.Token)
												 .ConfigureAwait(false);
					CurrentUserAccount = result?.Account;
				} catch (MsalUiRequiredException) {
					continue;
				} catch	(Exception ex) {
					Logger.ShowException(ex);
					continue;
				}
				yield return result?.AccessToken;
			}
		}

		public async Task<string> GetAccessTokenSilently(IAccount userAccount = null) {
			CurrentUserAccount = userAccount ??
				CurrentUserAccount ??
				(await msalClient.GetAccountsAsync().ConfigureAwait(false)).FirstOrDefault();

			if (CurrentUserAccount == null) {
				return null;
			}
			try {
				using var cts = new CancellationTokenSource(Timeouts.Silent);
				// If there is an account, call AcquireTokenSilent
				// By doing this, MSAL will refresh the token automatically if
				// it is expired. Otherwise it returns the cached token.
				var result = await msalClient.AcquireTokenSilent(Scopes, CurrentUserAccount)
											 .ExecuteAsync(cts.Token)
											 .ConfigureAwait(false);
				CurrentUserAccount = result?.Account;
				return result?.AccessToken;
			} catch (MsalUiRequiredException) {
				return null;
			} catch (MsalException ex) {
				Logger.ShowException(ex);
				return null;
			}
		}

		public async Task<string> GetAccessTokenInteractively() {
			using var cts = new CancellationTokenSource(Timeouts.Interactive);
			try {
				var result = await msalClient.AcquireTokenInteractive(Scopes)				 
					.ExecuteAsync(cts.Token)
					.ConfigureAwait(false);
				CurrentUserAccount = result?.Account;
				return result?.AccessToken;
			} catch (MsalException ex) {
				Logger.ShowException(ex);
				return null;
			}
		}

		/// <summary>
		/// This method can only be used with <see cref="Authority.Organizations"/> 
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
				CurrentUserAccount = result?.Account;
				return result?.AccessToken;
			} catch (MsalException ex) {
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
			var url = request.RequestUri.ToString().ToLower();
			string token;
			if (!url.Contains("users")) {
				token = await GetAccessTokenSilently().ConfigureAwait(false);
			} else {
				var paths = url.Split('/').ToList();
				var i = paths.IndexOf("users");
				var userId = paths[i + 1];
				if (!UserIdAccountRegistry.ContainsKey(userId)) {
					throw new InvalidOperationException();
				}
				var userAccount = UserIdAccountRegistry[userId];
				token = await GetAccessTokenSilently(userAccount).ConfigureAwait(false);
			}
			// attach authentication to the header of http request
			request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
		}

		public async Task<bool> LogoutAsync(string userId) {
			var account = UserIdAccountRegistry[userId];
			if (!UserIdAccountRegistry.ContainsKey(userId)) {
				throw new InvalidOperationException();
			}
			try {
				await msalClient.RemoveAsync(account).ConfigureAwait(false);
				UserIdAccountRegistry.Remove(userId);
				return true;
			} catch (MsalException ex) {
				Logger.ShowException(ex);
				return false;
			}
		}

	}
}