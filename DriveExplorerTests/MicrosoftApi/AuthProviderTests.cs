using Xunit;
using System;
using DriveExplorer.IoC;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Identity.Client;

namespace DriveExplorer.MicrosoftApi {
    public class AuthProviderTests{

		[Fact]
		public async Task WithUsernameInAppConfig_GetAccessTokenWithUsername_ResultNotNullAsync()
		{
			//Given
			var authProvider = new AuthProvider(AuthProvider.Authority.Organizations);
			//When
			var token = await authProvider.GetAccessTokenWithUsernamePassword();
			//Then
			Assert.NotNull(token);
		}

        [Fact(Skip = "Need user interaction")]
        public async Task GetAccessToken_SuccessWithInteractive() {
            var authProvider = AuthProvider.Default;
            try {
                var token = await authProvider.GetAccessToken();
                Assert.NotNull(token);
            } catch (OperationCanceledException ex) {
                Debug.WriteLine(ex.Message);
            }
        }


    }
}
