using System.Diagnostics;
using Xunit;
using System;
using Microsoft.Graph;
using System.Linq;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DriveExplorer.MicrosoftApi {
    public class GraphManagerFixture {
        public AuthProvider authProvider;
        public GraphManager graphManager;

        public GraphManagerFixture() {
            authProvider = new AuthProvider(AuthProvider.Authority.Organizations);
            graphManager = new GraphManager(authProvider);
            var _ = authProvider.GetAccessTokenWithUsernamePassword().Result;
        }
    }

    [CollectionDefinition(Name)]
    public class GraphManagerCollection : ICollectionFixture<GraphManagerFixture>{
        public const string Name = "GraphManager";
    }

    [Collection(GraphManagerCollection.Name)]
    public class GraphManagerTests {
        private readonly AuthProvider authProvider;
        private readonly GraphManager graphManager;

        public GraphManagerTests(GraphManagerFixture fixture) {
            authProvider = fixture.authProvider;
            graphManager = fixture.graphManager;
            Debug.WriteLine(graphManager.GetHashCode());
        }

        [Fact]
        public async Task SearchDrive_ResultNotNullAsync() {
            //Given
            authProvider.Scopes = new[] { AuthProvider.Permissions.Files.Read };
            //When
            var file = (await graphManager.SearchDriveAsync("LICENSE.txt",
                new[] {
                    new QueryOption("$top", "5"),
                    new QueryOption("$select", GraphManager.Selects.name + "," + GraphManager.Selects.id)
                    })).First();
            Debug.WriteLine(file.Name);
            //Then
            Assert.NotNull(file);
        }

        [Fact]
        public async Task DownloadFile_ResultNotNullAsync() {
            //Given
            authProvider.Scopes = new[] { AuthProvider.Permissions.Files.Read };
            var item = (await graphManager.SearchDriveAsync("LICENSE.txt")).CurrentPage.First();
            //When
            var stream = await graphManager.GetFileAsync(item.Id);
            string content;
            using var reader = new StreamReader(stream);
            content = reader.ReadToEnd();
            Debug.WriteLine(content);
            //Then
            Assert.True(!string.IsNullOrEmpty(content));
        }

        [Fact]
        public async Task GetDriveRoot_ResultNotNullAsync() {
            //Given
            authProvider.Scopes = new[] { AuthProvider.Permissions.Files.Read };
            //When
            var item = await graphManager.GetDriveRootAsync();
            Debug.WriteLine(item.Name);
            //Then
            Assert.NotNull(item);
        }

        [Fact]
        public async Task GetChildrenOfRoot_ResultNotNullAsync() {
            //Given
            authProvider.Scopes = new[] { AuthProvider.Permissions.Files.Read };
            var root = await graphManager.GetDriveRootAsync();
            //When
            var children = graphManager.GetChildrenAsync(root.Id);
            await foreach (var child in children) {
                Debug.WriteLine(child.Name);
            }
            //Then
            Assert.NotNull(children);
        }

        [Fact]
        public async Task UpdateFile_ResultNotNullAsync() {
            //Given
            authProvider.Scopes = new[] { AuthProvider.Permissions.Files.ReadWrite };
            var itemId = (await graphManager.SearchDriveAsync("LICENSE.txt")).FirstOrDefault()?.Id;
            var content = "aaa";
            //When
            var driveItem = await graphManager.UpdateFileAsync(itemId, content);
            Debug.WriteLine(driveItem.Name);
            //Then
            Assert.NotNull(driveItem);
        }

        [Fact]
        public async Task UploadFile_ResultNotNullAsync() {
            //Given
            authProvider.Scopes = new[] { AuthProvider.Permissions.Files.ReadWrite };
            var content = "aaa";
            var parentId = (await graphManager.GetDriveRootAsync()).Id;
            var filename = "aaa.txt";
            //When
            var response = await graphManager.UploadFileAsync(parentId, filename, content);
            Debug.WriteLine(response);
            //Then
            Assert.NotNull(response);
        }
    }

    [Collection(GraphManagerCollection.Name)]
    public class GraphApiCallTests {
        private readonly AuthProvider authProvider;

        public GraphApiCallTests(GraphManagerFixture fixture) {
            authProvider = fixture.authProvider;
        }

        class User {
            [JsonProperty("@odata.context")]
            public string context;
            public string[] businessPhones;
            public string displayName;
            public string givenName;
            public string jobTitle;
            public string mail;
            public string mobilePhone;
            public string officeLocation;
            public string preferredLanguage;
            public string surname;
            public string userPrincipalName;
            public string id;
        }

        [Fact]
        public async Task GetUserApiCall_ResultNotNullAsync() {
            //Given
            authProvider.Scopes = new[] { AuthProvider.Permissions.User.Read };
            var url = GraphManager.ApiEndpoint + "me/";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            await authProvider.AuthenticateRequestAsync(request);
            //When
            using var client = new HttpClient
            {
                Timeout = GraphManager.Timeouts.Silent,
            };
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(responseBody);
            var jObject = JObject.Parse(responseBody);
            var jString = jObject.ToString();
            Debug.WriteLine(jString);
            var user = JsonConvert.DeserializeObject<User>(responseBody);
            //Then
            Assert.NotNull(responseBody);
        }
    }
}