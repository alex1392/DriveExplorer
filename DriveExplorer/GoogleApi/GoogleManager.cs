using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DriveExplorer.GoogleApi {
	public class GoogleManager {
		private class Timeouts {
			public static readonly TimeSpan Silent = TimeSpan.FromSeconds(10);
		}
		private const string ClientSecretsPath = "GoogleApi/client_secret.json";


		private readonly string[] scopes;
		private readonly UserCredential credential;
		private readonly DriveService service;

		public GoogleManager() {
			scopes = new[] { DriveService.Scope.Drive };
			credential = FromFileAsync(
				ClientSecretsPath,
				scopes).Result;
			service = new DriveService(
				new BaseClientService.Initializer
				{
					HttpClientInitializer = credential,
					ApplicationName = nameof(DriveExplorer),
				});
		}

		/// <summary>
		/// Get UserCredential from file.
		/// </summary>
		/// <param name="path">The path of user credential file.</param>
		/// <param name="scopes">The scopes needed for the client application.</param>
		/// <returns>UserCredential class</returns>
		private static async Task<UserCredential> FromFileAsync(string path, IEnumerable<string> scopes) {
			using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			return await GoogleWebAuthorizationBroker.AuthorizeAsync(
				GoogleClientSecrets.Load(stream).Secrets,
				new[] { DriveService.Scope.Drive },
				"user",
				cts.Token).ConfigureAwait(false);
		}
	}
}