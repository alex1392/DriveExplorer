using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DriveExplorer.MicrosoftApi {
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
            var user = await client.Me.Request().GetAsync(Timeouts.Silent).ConfigureAwait(false);
            UserCache = user;
            return user;
        }
        public async Task<DriveItem> GetDriveRootAsync() {
            return await client.Me.Drive.Root.Request().GetAsync(Timeouts.Silent).ConfigureAwait(false);
        }

        /// <summary>
        /// Requires scopes: <see cref="Permissions.Files.Read"/>
        /// </summary>
        /// <param name="query"><see cref="string"/> of search query</param>
        /// <param name="options"><see cref="IEnumerable{T}"/> of strings to select</param>
        /// <returns></returns>
        public async Task<IDriveItemSearchCollectionPage> SearchDriveAsync(string query, IEnumerable<QueryOption> options = null) {
            return await client.Me.Drive.Root.Search(query).Request(options).GetAsync(Timeouts.Silent).ConfigureAwait(false);
        }

        public async Task<Stream> GetFileAsync(string id) {
            return await client.Me.Drive.Items[id].Content.Request().GetAsync(Timeouts.Silent).ConfigureAwait(false);
        }

        /// <summary>
        /// Get subfolders and files in a folder with <paramref name="parentId"/>
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public async Task<(List<DriveItem> folders, List<DriveItem> files)> GetFolersFilesAsync(string parentId) {
            var folders = new List<DriveItem>();
            var files = new List<DriveItem>();
            var request = client.Me.Drive.Items[parentId].Children.Request();
            do {
                var page = await request.GetAsync(Timeouts.Silent).ConfigureAwait(false);
                folders.AddRange(page.Where(item => item.Folder != null));
                files.AddRange(page.Where(item => item.File != null));
                request = page.NextPageRequest;
            } while (request != null);
            return (folders, files);
        }

        public async IAsyncEnumerable<DriveItem> GetFolersAsync(string parentId) {
            var request = client.Me.Drive.Items[parentId].Children.Request();
            do {
                var page = await request.GetAsync(Timeouts.Silent).ConfigureAwait(false);
                foreach (var folder in page.Where(item => item.Folder != null)) {
                    yield return folder;
                }
                request = page.NextPageRequest;
            } while (request != null);
        }

        public async IAsyncEnumerable<DriveItem> GetFilesAsync(string parentId) {
            var request = client.Me.Drive.Items[parentId].Children.Request();
            do {
                var page = await request.GetAsync(Timeouts.Silent).ConfigureAwait(false);
                foreach (var file in page.Where(item => item.File != null)) {
                    yield return file;
                }
                request = page.NextPageRequest;
            } while (request != null);
        }

        public async IAsyncEnumerable<DriveItem> GetChildrenAsync(string parentId) {
            var request = client.Me.Drive.Items[parentId].Children.Request();
            do {
                var page = await request.GetAsync(Timeouts.Silent).ConfigureAwait(false);
                foreach (var file in page) {
                    yield return file;
                }
                request = page.NextPageRequest;
            } while (request != null);
        }

        public async Task<string> UploadFileAsync(string parentId, string filename, string content) {
            var urlString = Urls.Graph + $"/me/drive/items/{parentId}:/{filename}:/content";
            var uri = new Uri(urlString);
            using var cts = new CancellationTokenSource(Timeouts.Silent);
            using var request = new HttpRequestMessage(HttpMethod.Put, uri);
            using var stringContent = new StringContent(content, Encoding.UTF8);
            request.Content = stringContent;
            using var response = await client.HttpProvider.SendAsync(request, HttpCompletionOption.ResponseContentRead, cts.Token).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        }

        public async Task<DriveItem> UpdateFileAsync(string itemId, string content) {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            using var cts = new CancellationTokenSource(Timeouts.Silent);
            return await client.Me.Drive.Items[itemId].Content.Request().PutAsync<DriveItem>(stream, cts.Token).ConfigureAwait(false);
        }
    }

    public static class RequestExtensions {
        /// <summary>
        /// Perform get request with specified timeout.
        /// </summary>
        public static async Task<IDriveItemChildrenCollectionPage> GetAsync(this IDriveItemChildrenCollectionRequest request, TimeSpan timeout) {
            var cts = new CancellationTokenSource(timeout);
            try {
                return await request.GetAsync(cts.Token).ConfigureAwait(false);
            } catch (TimeoutException ex) {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public static async Task<IDriveItemSearchCollectionPage> GetAsync(this IDriveItemSearchRequest request, TimeSpan timeout) {
            var cts = new CancellationTokenSource(timeout);
            try {
                return await request.GetAsync(cts.Token).ConfigureAwait(false);
            } catch (TimeoutException ex) {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public static async Task<User> GetAsync(this IUserRequest request, TimeSpan timeout) {
            var cts = new CancellationTokenSource(timeout);
            try {
                return await request.GetAsync(cts.Token).ConfigureAwait(false);
            } catch (TimeoutException ex) {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public static async Task<DriveItem> GetAsync(this IDriveItemRequest request, TimeSpan timeout) {
            var cts = new CancellationTokenSource(timeout);
            try {
                return await request.GetAsync(cts.Token).ConfigureAwait(false);
            } catch (TimeoutException ex) {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public static async Task<Stream> GetAsync(this IDriveItemContentRequest request, TimeSpan timeout) {
            var cts = new CancellationTokenSource(timeout);
            try {
                return await request.GetAsync(cts.Token).ConfigureAwait(false);
            } catch (TimeoutException ex) {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
    }
}