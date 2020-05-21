using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Extensions;

namespace DriveExplorer.MicrosoftApi {
	public class GraphManager {
		private static GraphServiceClient client;
		private static GraphManager instance;
		public static GraphManager Instance => instance;

		// lazy singleton
		public static void Initialize(IAuthenticationProvider authProvider) {
			if (instance == null) {
				instance = new GraphManager(authProvider);
			} else {
				Console.WriteLine($"{typeof(GraphManager)} has already been initialized.");
			}
		}

		private GraphManager(IAuthenticationProvider authProvider) {
			client = new GraphServiceClient(authProvider);

		}

		public async Task<User> GetMeAsync() {
			using (var cts = new CancellationTokenSource(Timeouts.Silent)) {
				return await client.Me.Request().GetAsync(cts.Token);
			}
		}

		/// <summary>
		/// Requires scopes: <see cref="Permissions.Files.Read"/>
		/// </summary>
		/// <param name="query"><see cref="System.String"/> of search query</param>
		/// <param name="options"><see cref="IEnumerable{T}"/> of strings to select</param>
		/// <returns></returns>
		public async Task<IDriveItemSearchCollectionPage> SearchDriveAsync(string query, IEnumerable<QueryOption> options = null) {
			using (var cts = new CancellationTokenSource(Timeouts.Silent)) {
				return await client.Me.Drive.Root.Search(query).Request(options).GetAsync(cts.Token);
			}
		}

		public async Task<Stream> GetFileAsync(string id) {
			using (var cts = new CancellationTokenSource(Timeouts.Silent)) {
				return await client.Me.Drive.Items[id].Content.Request().GetAsync(cts.Token);
			}

		}

		public async Task<string> UploadFileAsync(string parentId, string filename, string content) {
			var urlString = Urls.BaseUrl + $"/me/drive/items/{parentId}:/{filename}:/content";
			var uri = new Uri(urlString);
			using (var cts = new CancellationTokenSource(Timeouts.Silent))
			using (var request = new HttpRequestMessage(HttpMethod.Put, uri))
			using (var stringContent = new StringContent(content, Encoding.UTF8)) {
				request.Content = stringContent;
				// await AuthProvider.Instance.AuthenticateRequestAsync(request);
				using (var response = await client.HttpProvider.SendAsync(request, HttpCompletionOption.ResponseContentRead, cts.Token)) {
					return await response.Content.ReadAsStringAsync();
				}
			}

		}

		public async Task<DriveItem> GetDriveRootAsync() {
			using (var cts = new CancellationTokenSource(Timeouts.Silent)) {
				return await client.Me.Drive.Root.Request().GetAsync(cts.Token);
			}
		}

		public async Task<DriveItem> UpdateFileAsync(string itemId, string content) {
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
			using (var cts = new CancellationTokenSource(Timeouts.Silent)) {
				return await client.Me.Drive.Items[itemId].Content.Request().PutAsync<DriveItem>(stream, cts.Token);
			}
		}

		public async Task<(List<DriveItem> folders, List<DriveItem> files)> GetFolersAndFilesAsync(string parentId) {
			using (var cts = new CancellationTokenSource(Timeouts.Silent)) {
				var folders = new List<DriveItem>();
				var files = new List<DriveItem>();
				IDriveItemChildrenCollectionPage page;
				do {
					page = await GetChildrenAsync(parentId);
					folders.AddRange(page.Where(item => item.Folder != null));
					files.AddRange(page.Where(item => item.File != null));
				} while (page.NextPageRequest != null);
				return (folders, files);
			}
		}

		/// <summary>
		/// Get subfolders and files in a folder with <paramref name="parentId"/>
		/// </summary>
		/// <param name="parentId"></param>
		/// <returns></returns>
		public async Task<IDriveItemChildrenCollectionPage> GetChildrenAsync(string parentId) {
			using (var cts = new CancellationTokenSource(Timeouts.Silent)) {
				return await client.Me.Drive.Items[parentId].Children.Request().GetAsync(cts.Token);
			}
		}
	}
}