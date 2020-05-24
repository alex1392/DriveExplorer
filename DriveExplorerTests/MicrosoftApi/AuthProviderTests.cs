using Xunit;
using Microsoft.Extensions.Configuration;
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
			var authProvider = new AuthProvider(new ConfigurationBuilder().AddUserSecrets<AuthProviderTests>().Build(), Urls.Auth.Organizations);
			//When
			var token = await authProvider.GetAccessTokenWithUsernamePassword();
			//Then
			Assert.NotNull(token);
		}
       
        [Fact]
        public void EmptyAppConfig_GetAuthProvider_Throws() {
            var appConfig = new ConfigurationBuilder().Build();
            Assert.Throws<ArgumentException>(() => new AuthProvider(appConfig));
        }

        [Fact]
        public async Task NoUsernameInAppConfig_GetAccessTokenWithUsername_NullTokenAsync() {
            var authProvider = new AuthProvider(null, Urls.Auth.Organizations);
			await Assert.ThrowsAsync<MsalClientException>(async () => await authProvider.GetAccessTokenWithUsernamePassword());
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
