using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DriveExplorer.MicrosoftApi {
	/// <summary>
	/// Handles all graph api calling, includes error handling. If there's any error occurs in the api call, returns null
	/// </summary>
	public class GraphManager {
		private readonly GraphServiceClient client;
		private User userCache = null;

		/// <summary>
		/// Get default <see cref="GraphManager"/> with <see cref="AuthProvider.Default"/>
		/// </summary>
		public static GraphManager Default { get; private set; } = new GraphManager();
		public User UserCache {
			get {
				if (userCache == null) {
					userCache = Task.Run(async () => await GetMeAsync()).Result;
				}
				return userCache;
			}
			private set => userCache = value;
		}
		public GraphManager(AuthProvider authProvider = null) {
			if (authProvider is null) {
				authProvider = AuthProvider.Default;
			}

			client = new GraphServiceClient(authProvider);
		}

		public async Task<User> GetMeAsync() {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				var user = await client.Me.Request().GetAsync(cts.Token).ConfigureAwait(false);
				UserCache = user;
				return user;
			} catch (Exception ex) {
				Logger.ShowException(ex);
				return null;
			}
		}

		public async Task<DriveItem> GetDriveRootAsync() {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				return await client.Me.Drive.Root.Request().GetAsync(cts.Token).ConfigureAwait(false);
			} catch (Exception ex) {
				Logger.ShowException(ex);
				return null;
			}
		}

		/// <summary>
		/// Requires scopes: <see cref="Permissions.Files.Read"/>
		/// </summary>
		/// <param name="query"><see cref="string"/> of search query</param>
		/// <param name="options"><see cref="IEnumerable{T}"/> of strings to select</param>
		/// <returns></returns>
		public async Task<IDriveItemSearchCollectionPage> SearchDriveAsync(string query, IEnumerable<QueryOption> options = null) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				return await client.Me.Drive.Root.Search(query).Request(options).GetAsync(cts.Token).ConfigureAwait(false);
			} catch (Exception ex) {
				Logger.ShowException(ex);
				return null;
			}
		}

		public async Task<Stream> GetFileAsync(string id) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				return await client.Me.Drive.Items[id].Content.Request().GetAsync(cts.Token).ConfigureAwait(false);
			} catch (Exception ex) {
				Logger.ShowException(ex);
				return null;
			}
		}

		public async IAsyncEnumerable<DriveItem> GetChildrenAsync(string parentId) {
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var request = client.Me.Drive.Items[parentId].Children.Request();
			do {
				IDriveItemChildrenCollectionPage page;
				try {
					page = await request.GetAsync(cts.Token).ConfigureAwait(false);
				} catch (Exception ex) {
					Logger.ShowException(ex);
					yield break;
				}
				foreach (var file in page) {
					yield return file;
				}
				request = page?.NextPageRequest;
			} while (request != null);
		}

		public async Task<string> UploadFileAsync(string parentId, string filename, string content) {
			var urlString = Urls.Graph + $"/me/drive/items/{parentId}:/{filename}:/content";
			var uri = new Uri(urlString);
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			using var request = new HttpRequestMessage(HttpMethod.Put, uri);
			using var stringContent = new StringContent(content, Encoding.UTF8);
			request.Content = stringContent;
			try {
				using var response = await client.HttpProvider.SendAsync(request, HttpCompletionOption.ResponseContentRead, cts.Token).ConfigureAwait(false);
				return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			} catch (Exception ex) {
				Logger.ShowException(ex);
				return null;
			}
		}

		public async Task<DriveItem> UpdateFileAsync(string itemId, string content) {
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var request = client.Me.Drive.Items[itemId].Content.Request();
			try {
				return await request.PutAsync<DriveItem>(stream, cts.Token).ConfigureAwait(false);
			} catch (Exception ex) {
				Logger.ShowException(ex);
				return null;
			}
		}
	}

}