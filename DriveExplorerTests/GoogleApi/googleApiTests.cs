using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Xunit;
using System.ComponentModel;
using System.Threading;
using Google;

namespace DriveExplorer.GoogleApi {
	public class GoogleApiTests {
		private const string ClientSecretsPath = "GoogleApi/client_secret.json";
		private readonly string[] scopes;
		private readonly UserCredential credential;
		private readonly DriveService service;

		public GoogleApiTests() {
			scopes = new[] { DriveService.Scope.Drive };
			credential = UserCredentialProvider.FromFileAsync(
				ClientSecretsPath,
				scopes).Result;
			service = new DriveService(
				new BaseClientService.Initializer {
					HttpClientInitializer = credential,
					ApplicationName = nameof(DriveExplorer),
				});
		}

		[Fact]
		public void OAuthTest() {
			Assert.NotNull(credential);
		}

		[Fact]
		public async Task GetAboutTestAsync() {
			//Given
			//When
			var getRequest = service.About.Get();
			getRequest.Fields = "*";
			var about = await getRequest.ExecuteAsync();
			foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(about)) {
				var name = descriptor.Name;
				var value = descriptor.GetValue(about);
				Console.WriteLine($"{name}: {value}");
			}
			//Then
			Assert.NotNull(about);
		}

		[Fact]
		public async Task GetAllDrivesTestAsync() {
			//Given
			//When
			var drives = new List<Drive>();
			var request = service.Drives.List();
			do {
				var list = await request.ExecuteAsync();
				drives.AddRange(list.Drives);
				request.PageToken = list.NextPageToken;
			} while (!string.IsNullOrEmpty(request.PageToken));
			foreach (var drive in drives) {
				Console.WriteLine(drive.Id + ":" + drive.Name);
			}
			//Then
			Assert.NotNull(drives);
		}

		[Fact]
		public async Task GetFilesTestAsync() {
			//Given

			//When
			var files = (await service.Files.List().ExecuteAsync()).Files;
			foreach (var file in files) {
				Console.WriteLine(file.Id + ":" + file.Name);
			}
			//Then
			Assert.NotNull(files);
		}

		[Fact(Skip = "Time comsuming")]
		public async Task GetAllFilesTestAsync() {
			//Given

			//When
			var files = new List<File>();
			var request = service.Files.List();
			do {
				var list = await request.ExecuteAsync();
				files.AddRange(list.Files);
				request.PageToken = list.NextPageToken;
			} while (!string.IsNullOrEmpty(request.PageToken));
			foreach (var file in files) {
				Console.WriteLine(file.Id + ":" + file.Name);
			}
			//Then
			Assert.NotNull(files);
		}

		[Fact]
		public async Task GetRootFolderTestAsync() {
			//Given
			//When
			var root = await service.Files.Get("root").ExecuteAsync();
			Console.WriteLine(root.Id);
			//Then
			Assert.NotNull(root);
		}

		[Fact]
		public async Task GetChildrenInRootTestAsync() {
			//Given
			var root = await service.Files.Get("root").ExecuteAsync();
			//When
			var request = service.Files.List();
			request.Q = $"parents in '{root.Id}' and trashed = false";
			var files = new List<File>();
			do {
				var fileList = await request.ExecuteAsync();
				files.AddRange(fileList.Files);
				request.PageToken = fileList.NextPageToken;
			} while (!string.IsNullOrEmpty(request.PageToken));
			foreach (var file in files)
			{
				Console.WriteLine(file.Id + ": " + file.Name);
			}
			//Then
			
		}
	}
}