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

    }
}
