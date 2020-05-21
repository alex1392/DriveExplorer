using System.Diagnostics;
using Xunit;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using System.Linq;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DriveExplorer.MicrosoftApi {
	public class GraphManagerTestFixture {
		public IConfigurationRoot appConfig;
		public AuthProvider authProvider;
		public GraphManager graphManager;

		public GraphManagerTestFixture() {
			appConfig = new ConfigurationBuilder()
							.AddUserSecrets<GraphManagerTests>()
							.Build();
			AuthProvider.Initialize(appConfig);
			GraphManager.Initialize(AuthProvider.Instance);
			authProvider = AuthProvider.Instance;
			graphManager = GraphManager.Instance;
		}

	}
	public class GraphManagerTests : IClassFixture<GraphManagerTestFixture> {
		private IConfigurationRoot appConfig;
		private AuthProvider authProvider;
		private GraphManager graphManager;

		public GraphManagerTests(GraphManagerTestFixture fixture) {
			appConfig = fixture.appConfig;
			authProvider = fixture.authProvider;
			graphManager = fixture.graphManager;
			Debug.WriteLine(graphManager.GetHashCode());
		}

		[Fact]
		public void GetGraphManager_EqualToOriginal() {
			//Given
			//When
			var graphManager1 = GraphManager.Instance;
			//Then
			Assert.Equal(graphManager, graphManager1);
		}

		[Fact]
		public void GetUser_UserMailEqualToUsernameInAppConfig() {
			//Given
			authProvider.Scopes = new[] { Permissions.User.Read };
			string Username = appConfig[nameof(Username)];
			//When
			var user = graphManager.GetMeAsync().Result;
			Console.WriteLine(user.DisplayName);
			//Then
			Assert.Equal(Username, user.Mail);
		}

		[Fact]
		public void SearchDrive_ResultNotNull() {
			//Given
			authProvider.Scopes = new[] { Permissions.Files.Read };
			//When
			var file = graphManager.SearchDriveAsync("LICENSE.txt",
				new[] {
					new QueryOption("$top", "5"),
					new QueryOption("$select", Selects.name + "," + Selects.id)
					}).Result.First();
			Console.WriteLine(file.Name);
			//Then
			Assert.NotNull(file);
		}

		[Fact]
		public void DownloadFile_ResultNotNull() {
			//Given
			authProvider.Scopes = new[] { Permissions.Files.Read };
			var item = graphManager.SearchDriveAsync("LICENSE.txt").Result.CurrentPage.First();
			//When
			var stream = graphManager.GetFileAsync(item.Id).Result;
			string content;
			using (var reader = new StreamReader(stream)) {
				content = reader.ReadToEnd();
				Console.WriteLine(content);
			}
			//Then
			Assert.True(!string.IsNullOrEmpty(content));
		}

		[Fact]
		public void GetDriveRoot_ResultNotNull() {
			//Given
			authProvider.Scopes = new[] { Permissions.Files.Read };
			//When
			var item = graphManager.GetDriveRootAsync().Result;
			Console.WriteLine(item.Name);
			//Then
			Assert.NotNull(item);
		}

		[Fact]
		public void GetChildrenOfRoot_ResultNotNull() {
			//Given
			authProvider.Scopes = new[] { Permissions.Files.Read };
			var root = graphManager.GetDriveRootAsync().Result;
			//When
			var children = graphManager.GetChildrenAsync(root.Id).Result;
			foreach (var child in children) {
				Console.WriteLine(child.Name);
			}
			//Then
			Assert.NotNull(children);
		}

		[Fact]
		public void GetFoldersAndFilesOfRoot_ResultNotNull() {
			//Given
			authProvider.Scopes = new[] { Permissions.Files.Read };
			var root = graphManager.GetDriveRootAsync().Result;
			//When
			var (folders, files) = graphManager.GetFolersAndFilesAsync(root.Id).Result;
			Console.WriteLine("=====Folders=====");
			folders.ForEach(folder => Console.WriteLine(folder.Name));
			Console.WriteLine("=====Files=====");
			files.ForEach(file => Console.WriteLine(file.Name));
			//Then
			Assert.True(folders != null || files != null);
		}

		[Fact]
		public void UpdateFile_ResultNotNull() {
			//Given
			authProvider.Scopes = new[] { Permissions.Files.ReadWrite };
			var itemId = graphManager.SearchDriveAsync("LICENSE.txt").Result.FirstOrDefault()?.Id;
			var content = "aaa";
			//When
			var driveItem = graphManager.UpdateFileAsync(itemId, content).Result;
			Console.WriteLine(driveItem.Name);
			//Then
			Assert.NotNull(driveItem);
		}

		[Fact]
		public void UploadFile_ResultNotNull() {
			//Given
			authProvider.Scopes = new[] { Permissions.Files.ReadWrite };
			var content = "aaa";
			var parentId = graphManager.GetDriveRootAsync().Result.Id;
			var filename = "aaa.txt";
			//When
			var response = graphManager.UploadFileAsync(parentId, filename, content).Result;
			Console.WriteLine(response);
			//Then
			Assert.NotNull(response);
		}
	}

	public class GraphApiCallTests {
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
		public void GetUserApiCall_ResultNotNull() {
			//Given
			var appConfig = new ConfigurationBuilder()
				.AddUserSecrets<GraphManagerTests>()
				.Build();
			AuthProvider.Initialize(appConfig);
			AuthProvider.Instance.Scopes = new[] { Permissions.User.Read };
			var url = $"{Urls.BaseUrl}me/";
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			AuthProvider.Instance.AuthenticateRequestAsync(request).Wait();
			//When
			TimeSpan timeout = TimeSpan.FromSeconds(5);
			HttpResponseMessage response;
			using (var client = new HttpClient()) {
				client.Timeout = timeout;
				response = client.SendAsync(request).Result;
			}
			var responseBody = response.Content.ReadAsStringAsync().Result;
			Console.WriteLine(responseBody);
			var jObject = JObject.Parse(responseBody);
			var jString = jObject.ToString();
			Console.WriteLine(jString);
			var user = JsonConvert.DeserializeObject<User>(responseBody);
			//Then
			Assert.NotNull(responseBody);
		}
	}
}