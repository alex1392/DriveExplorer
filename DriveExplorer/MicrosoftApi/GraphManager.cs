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
		private readonly List<IAccount> accountList = new List<IAccount>();
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

		public async IAsyncEnumerable<(string, IAccount)> GetAllAccessTokenSilently() {
			var accounts = await msalClient.GetAccountsAsync().ConfigureAwait(false);
			foreach (var account in accounts) {
				AuthenticationResult result;
				try {
					using var cts = new CancellationTokenSource(Timeouts.Silent);
					result = await msalClient.AcquireTokenSilent(Scopes, account)
												 .ExecuteAsync(cts.Token)
												 .ConfigureAwait(false);

					RegisterUser(result?.Account);
				} catch (MsalUiRequiredException) {
					continue;
				} catch (Exception ex) {
					logger.Log(ex);
					continue;
				}
				yield return (result?.AccessToken, result?.Account);
			}
		}

		public async Task<(string, IAccount)> GetAccessTokenInteractively(string claims = null) {
			using var cts = new CancellationTokenSource(Timeouts.Interactive);
			try {
				var requestBuilder = msalClient.AcquireTokenInteractive(Scopes);
				if (claims != null) {
					requestBuilder = requestBuilder.WithClaims(claims);
				}
				var result = await requestBuilder
					.ExecuteAsync(cts.Token)
					.ConfigureAwait(false);

				RegisterUser(result?.Account);
				return (result?.AccessToken, result?.Account);
			} catch (Exception ex) {
				logger.Log(ex);
				return (null, null);
			}
		}

		/// <summary>
		/// This method can only be used with <see cref="Authority.Organizations"/> 
		/// </summary>
		public async Task<(string, IAccount)> GetAccessTokenWithUsernamePassword() {
			if (username == null || password == null) {
				return (null, null);
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
				RegisterUser(result?.Account);
				return (result?.AccessToken, result?.Account);
			} catch (Exception ex) {
				logger.Log(ex);
				return (null, null);
			}
		}

		private void RegisterUser(IAccount account) {
			if (accountList.Contains(account)) {
				logger.Log(new InvalidOperationException("The user has already signed in."));
				return;
			}
			accountList.Add(account);
		}

		private async Task<string> GetAccessTokenSilently(IAccount account) {
			try {
				using var cts = new CancellationTokenSource(Timeouts.Silent);
				// If there is an account, call AcquireTokenSilent
				// By doing this, MSAL will refresh the token automatically if
				// it is expired. Otherwise it returns the cached token.
				var result = await msalClient.AcquireTokenSilent(Scopes, account)
											 .ExecuteAsync(cts.Token)
											 .ConfigureAwait(false);

				return result?.AccessToken;
			} catch (MsalUiRequiredException ex) {
				var (token, _) = await GetAccessTokenInteractively(ex.Claims).ConfigureAwait(false);
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
			var url = request.RequestUri.ToString().ToLower();
			if (!url.Contains("users")) {
				throw new InvalidOperationException("The request doesn't specify a user");
			}
			var userId = GetUserId(url);
			var userAccount = GetAccount(userId);
			var token = await GetAccessTokenSilently(userAccount).ConfigureAwait(false);
			// attach authentication to the header of http request
			request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
		}

		public async Task<bool> LogoutAsync(IAccount account) {
			try {
				// TODO: this method just clears the cache without truely logout the user!!
				await msalClient.RemoveAsync(account).ConfigureAwait(false);
				accountList.Remove(account);
				return true;
			} catch (Exception ex) {
				logger.Log(ex);
				return false;
			}
		}
		public async Task<User> GetUserAsync(IAccount account) {
			var userId = GetUserId(account);
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				var user = await graphClient.Users[userId].Request().GetAsync(cts.Token).ConfigureAwait(false);
				return user;
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}
		public async Task<DriveItem> GetDriveRootAsync(IAccount account) {
			var userId = GetUserId(account);
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				return await graphClient.Users[userId].Drive.Root.Request().GetAsync(cts.Token).ConfigureAwait(false);
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}
		public async IAsyncEnumerable<DriveItem> GetChildrenAsync(string parentId, IAccount account) {
			var userId = GetUserId(account);
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var request = graphClient.Users[userId].Drive.Items[parentId].Children.Request();
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
		private async Task<User> GetUserAsync(string userId) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				var user = await graphClient.Users[userId].Request().GetAsync(cts.Token).ConfigureAwait(false);
				return user;
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}
		private async Task<DriveItem> GetDriveRootAsync(string userId) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				return await graphClient.Users[userId].Drive.Root.Request().GetAsync(cts.Token).ConfigureAwait(false);
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}
		private async IAsyncEnumerable<DriveItem> GetChildrenAsync(string parentId, string userId) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var request = graphClient.Users[userId].Drive.Items[parentId].Children.Request();
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
		private async Task<User> GetUserAsync() {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				var user = await graphClient.Me.Request().GetAsync(cts.Token).ConfigureAwait(false);
				return user;
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}
		private async Task<DriveItem> GetDriveRootAsync() {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				return await graphClient.Me.Drive.Root.Request().GetAsync(cts.Token).ConfigureAwait(false);
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}
		private async IAsyncEnumerable<DriveItem> GetChildrenAsync(string parentId) {
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
		private static string GetUserId(string url) {
			var paths = url.Split('/').ToList();
			var i = paths.IndexOf("users");
			var userId = paths[i + 1];
			return userId;
		}
		private static string GetUserId(IAccount account) {
			var id = account.HomeAccountId.ObjectId;
			while (id.StartsWith("0")) {
				id = string.Concat(id.SkipWhile(c => c == '0')).Remove(0, 1);
			}
			if (string.IsNullOrEmpty(id)) {
				throw new InvalidOperationException("Cannot get user id");
			}
			return id;
		}
		private IAccount GetAccount(string userId) {
			var account = accountList.FirstOrDefault(account => GetUserId(account) == userId);
			if (account == null) {
				throw new InvalidOperationException("Cannot get account");
			}
			return account;
		}
	}

}