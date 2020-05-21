using Xunit;
using Microsoft.Extensions.Configuration;

namespace DriveExplorer.MicrosoftApi
{
	public class AuthProviderTests
	{
		private IConfigurationRoot appConfig;
		private AuthProvider authProvider;

		public AuthProviderTests(){
			appConfig = new ConfigurationBuilder()
				.AddUserSecrets<GraphManagerTests>()
				.Build();
			AuthProvider.Initialize(appConfig);
			authProvider = AuthProvider.Instance;
		}

		[Fact]
		public void GetAuthProvider_EqualToOriginal()
		{
			//Given
			//When
			var authProvider1 = AuthProvider.Instance;
			//Then
			Assert.Equal(authProvider, authProvider1);

		}
		
		[Fact]
		public void GetAccessTokenWithUsernamePassword_ResultNotNull()
		{
			//Given
			//When
			var token = authProvider.GetAccessTokenWithUsernamePassword().Result;
			//Then
			Assert.NotNull(token);
		}
	}
}
