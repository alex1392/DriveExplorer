using Microsoft.Graph;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DriveExplorer.MicrosoftApi {
	/// <summary>
	/// Handles all graph api calling, includes error handling. If there's any error occurs in the api call, returns null
	/// </summary>
	public class GraphManager {
		#region Constants
		public const string ApiEndpoint = "https://graph.microsoft.com/v1.0/";
		public static class Timeouts {
			public static readonly TimeSpan Silent = TimeSpan.FromSeconds(10);
			public static readonly TimeSpan Interactive = TimeSpan.FromMinutes(1);
		}
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

		#endregion
		
		private readonly AuthProvider authProvider;
		private readonly GraphServiceClient client;
		private readonly ILogger logger;

		public GraphManager(AuthProvider authProvider, ILogger logger) {
			this.authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
			client = new GraphServiceClient(authProvider);
			this.logger = logger;
		}

		public async Task<User> GetMeAsync(string userId = null) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				var requestBuilder = userId == null ? client.Me : client.Users[userId];
				return await requestBuilder.Request().GetAsync(cts.Token).ConfigureAwait(false);
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}

		public async Task<DriveItem> GetDriveRootAsync(string userId = null) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				var requestBuilder = userId == null ? client.Me : client.Users[userId];
				return await requestBuilder.Drive.Root.Request().GetAsync(cts.Token).ConfigureAwait(false);
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}

		public async Task<IDriveItemSearchCollectionPage> SearchDriveAsync(string query, IEnumerable<QueryOption> options = null, string userId = null) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				var requestBuilder = userId == null ? client.Me : client.Users[userId];
				return await requestBuilder.Drive.Root.Search(query).Request(options).GetAsync(cts.Token).ConfigureAwait(false);
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}

		public async Task<Stream> GetFileAsync(string fileId, string userId = null) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				var requestBuilder = userId == null ? client.Me : client.Users[userId];
				return await requestBuilder.Drive.Items[fileId].Content.Request().GetAsync(cts.Token).ConfigureAwait(false);
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}

		public async IAsyncEnumerable<DriveItem> GetChildrenAsync(string parentId, string userId = null) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var requestBuilder = userId == null ? client.Me : client.Users[userId];
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

		public async Task<string> UploadFileAsync(string parentId, string filename, string content, string userId = null) {
			var userStr = userId == null ? "/me" : $"/users/{userId}";
			var urlString = ApiEndpoint + $"{userStr}/drive/items/{parentId}:/{filename}:/content";
			var uri = new Uri(urlString);
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			using var request = new HttpRequestMessage(HttpMethod.Put, uri);
			using var stringContent = new StringContent(content, Encoding.UTF8);
			request.Content = stringContent;
			try {
				using var response = await client.HttpProvider.SendAsync(request, HttpCompletionOption.ResponseContentRead, cts.Token).ConfigureAwait(false);
				return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}

		public async Task<DriveItem> UpdateFileAsync(string itemId, string content, string userId = null) {
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var requestBuilder = userId == null ? client.Me : client.Users[userId];
			var request = requestBuilder.Drive.Items[itemId].Content.Request();
			try {
				return await request.PutAsync<DriveItem>(stream, cts.Token).ConfigureAwait(false);
			} catch (Exception ex) {
				logger.Log(ex);
				return null;
			}
		}
	}

}