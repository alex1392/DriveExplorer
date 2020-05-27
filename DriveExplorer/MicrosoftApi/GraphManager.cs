using Microsoft.Graph;
using Microsoft.Identity.Client;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DriveExplorer.MicrosoftApi {
	/// <summary>
	/// Handles all graph api calling, includes error handling. If there's any error occurs in the api call, returns null
	/// </summary>
	public class GraphManager : IAuthenticationProvider {
		#region Constants
		public const string ApiEndpoint = "https://graph.microsoft.com/v1.0/";
		public static class Selects {
			public const string id = "id";
			public const string name = "name";
			public const string size = "size";
			public const string webUrl = "webUrl";
			/// <summary>
			/// Not working
			/// </summary>
			public const string content = "content";
			public const string createdDateTime = "createdDateTime";
			/// <summary>
			/// Not working
			/// </summary>
			public const string downloadUrl = "@microsoft.graph.downloadUrl";
		}

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
		#endregion

		private readonly GraphServiceClient graphClient;
		private readonly IPublicClientApplication msalClient;
		private readonly ILogger logger;
		private readonly string appId;
		private readonly string username;
		private readonly string password;
		private readonly Dictionary<string, IAccount> userRegistry = new Dictionary<string, IAccount>();
		private IAccount currentAccount;
		/// <summary>
		/// Specifies the scopes of graph api would be used in the current application.
		/// </summary>
		public string[] Scopes { get; set; }
		/// <summary>
		/// Should be set before everytime <see cref="GraphManager"/> makes a call
		/// </summary>
		/// <summary>
		/// This property is used to find the corresponding <see cref="IAccount"/> from the given user id. They should be registered just after an user logged in and be removed just after an user logged out.
		/// </summary>

		public GraphManager(ILogger logger = null, string authority = Authority.Common) {
			graphClient = new GraphServiceClient(this);
			this.logger = logger;

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
				.Build();
			TokenCacheHelper.EnableSerialization(msalClient.UserTokenCache);

			bool ContainsKey(NameValueCollection appConfig, string key) {
				return appConfig.AllKeys.Any(item => item == key);
			}
		}

		public async IAsyncEnumerable<(string, IAccount, User)> GetAllAccessTokenSilently() {
			var accounts = await msalClient.GetAccountsAsync().ConfigureAwait(false);
			foreach (var account in accounts) {
				AuthenticationResult result;
				User user;
				try {
					using var cts = new CancellationTokenSource(Timeouts.Silent);
					result = await msalClient.AcquireTokenSilent(Scopes, account)
												 .ExecuteAsync(cts.Token)
												 .ConfigureAwait(false);
					currentAccount = result?.Account;
					user = await RegisterUserAsync(result?.Account).ConfigureAwait(false);
				} catch (MsalUiRequiredException) {
					continue;
				} catch (Exception ex) {
					logger.Log(ex);
					continue;
				}
				yield return (result?.AccessToken, result?.Account, user);
			}
		}

		public async Task<(string, IAccount, User)> GetAccessTokenInteractively(string claims = null) {
			using var cts = new CancellationTokenSource(Timeouts.Interactive);
			try {
				var requestBuilder = msalClient.AcquireTokenInteractive(Scopes);
				if (claims != null) {
					requestBuilder = requestBuilder.WithClaims(claims);
				}
				var result = await requestBuilder
					.ExecuteAsync(cts.Token)
					.ConfigureAwait(false);
				currentAccount = result?.Account;
				var user = await RegisterUserAsync(result?.Account).ConfigureAwait(false);
				return (result?.AccessToken, result?.Account, user);
			} catch (Exception ex) {
				logger.Log(ex);
				return (null, null, null);
			}
		}

		/// <summary>
		/// This method can only be used with <see cref="Authority.Organizations"/> 
		/// </summary>
		public async Task<(string, IAccount, User)> GetAccessTokenWithUsernamePassword() {
			if (username == null || password == null) {
				return (null, null, null);
			}
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var secureString = new SecureString();
			foreach (var c in password ?? "") {
				secureString.AppendChar(c);
			}
			try {
				var result = await msalClient
					.AcquireTokenByUsernamePassword(Scopes, username, secureString)
					.ExecuteAsync(cts.Token).ConfigureAwait(false);
				;
				currentAccount = result?.Account;
				var user = await RegisterUserAsync(result?.Account).ConfigureAwait(false);
				return (result?.AccessToken, result?.Account, user);
			} catch (Exception ex) {
				logger.Log(ex);
				return (null, null, null);
			}
		}

		private async Task<User> RegisterUserAsync(IAccount account) {
			if (userRegistry.Values.Contains(account)) {
				return null;
			}
			var user = await GetUserAsync().ConfigureAwait(false);
			userRegistry.Add(user.Id, account);
			return user;
		}

		private async Task<string> GetAccessTokenSilently() {
			if (currentAccount == null) {
				return null;
			}
			try {
				using var cts = new CancellationTokenSource(Timeouts.Silent);
				// If there is an account, call AcquireTokenSilent
				// By doing this, MSAL will refresh the token automatically if
				// it is expired. Otherwise it returns the cached token.
				var result = await msalClient.AcquireTokenSilent(Scopes, currentAccount)
											 .ExecuteAsync(cts.Token)
											 .ConfigureAwait(false);
				currentAccount = result?.Account;
				return result?.AccessToken;
			} catch (MsalUiRequiredException ex) {
				var (token, _, _) = await GetAccessTokenInteractively(ex.Claims).ConfigureAwait(false);
				return token;
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}
		/// <summary>
		/// Implementation of <see cref="IAuthenticationProvider"/>. This method is called everytime when <see cref="GraphManager"/> make a request.
		/// </summary>
		public async Task AuthenticateRequestAsync(HttpRequestMessage request) {
			// attach authentication to the header of http request
			request.Headers.Authorization = new AuthenticationHeaderValue("bearer", await GetAccessTokenSilently().ConfigureAwait(false));
		}

		public async Task<bool> LogoutAsync(string userId) {
			try {
				// TODO: this method just clears the cache without truely logout the user!!
				var account = userRegistry[userId];
				await msalClient.RemoveAsync(account).ConfigureAwait(false);
				userRegistry.Remove(userId);
				return true;
			} catch (Exception ex) {
				logger.Log(ex);
				return false;
			}
		}

		public async Task<User> GetUserAsync(string userId) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				currentAccount = userRegistry[userId];
				var user = await graphClient.Users[userId].Request().GetAsync(cts.Token).ConfigureAwait(false);
				return user;
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}
		public async Task<User> GetUserAsync() {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				var user = await graphClient.Me.Request().GetAsync(cts.Token).ConfigureAwait(false);
				return user;
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}
		public async Task<DriveItem> GetDriveRootAsync(string userId) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				var requestBuilder = graphClient.Users[userId];
				currentAccount = userRegistry[userId];
				return await requestBuilder.Drive.Root.Request().GetAsync(cts.Token).ConfigureAwait(false);
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}
		public async Task<DriveItem> GetDriveRootAsync() {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				var requestBuilder = graphClient.Me;
				return await requestBuilder.Drive.Root.Request().GetAsync(cts.Token).ConfigureAwait(false);
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}
		public async IAsyncEnumerable<DriveItem> GetChildrenAsync(string parentId, string userId) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var requestBuilder = graphClient.Users[userId];
			currentAccount = userRegistry[userId];
			var request = requestBuilder.Drive.Items[parentId].Children.Request();
			do {
				IDriveItemChildrenCollectionPage page;
				try {
					page = await request.GetAsync(cts.Token).ConfigureAwait(false);
				} catch (Exception ex) {
					logger.Log(ex);
					yield break;
				}
				foreach (var file in page) {
					yield return file;
				}
				request = page?.NextPageRequest;
			} while (request != null);
		}
		public async IAsyncEnumerable<DriveItem> GetChildrenAsync(string parentId) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var requestBuilder = graphClient.Me;
			var request = requestBuilder.Drive.Items[parentId].Children.Request();
			do {
				IDriveItemChildrenCollectionPage page;
				try {
					page = await request.GetAsync(cts.Token).ConfigureAwait(false);
				} catch (Exception ex) {
					logger.Log(ex);
					yield break;
				}
				foreach (var file in page) {
					yield return file;
				}
				request = page?.NextPageRequest;
			} while (request != null);
		}
	}

}