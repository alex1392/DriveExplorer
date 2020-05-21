using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;

namespace DriveExplorer.GoogleApi {
	public class UserCredentialProvider {
		/// <summary>
		/// Get UserCredential from file.
		/// </summary>
		/// <param name="path">The path of user credential file.</param>
		/// <param name="scopes">The scopes needed for the client application.</param>
		/// <returns>UserCredential class</returns>
		public static async Task<UserCredential> FromFileAsync(string path, IEnumerable<string> scopes) {
			using(var stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
				return await GoogleWebAuthorizationBroker.AuthorizeAsync(
					GoogleClientSecrets.Load(stream).Secrets,
					new [] { DriveService.Scope.Drive },
					"user",
					CancellationToken.None);
			}
		}
	}
}