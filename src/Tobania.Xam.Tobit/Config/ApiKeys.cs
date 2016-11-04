using System;
namespace Tobania.Xam.Tobit.Config
{
	public static class ApiKeys
	{
		public const string ClientId = "<YOUR-CLIENT-ID>";
		public const string ClientSecret = "<YOUR-CLIENT-SECRET>";
		public const string RedirectUrl = "<YOUR-REDIRECT-URL>";
		public const string Scope = "user repo";
		public const string AuthorizeUrl = "https://github.com/login/oauth/authorize";
		public const string AccessTokenUrl = "https://github.com/login/oauth/access_token";
		public const string GitHubApi = "https://api.github.com";
		public const string AccessToken = "access_token";
	}
}
