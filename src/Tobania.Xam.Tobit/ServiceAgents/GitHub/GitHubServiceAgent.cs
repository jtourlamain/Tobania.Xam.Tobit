using System;
using System.Net.Http;
using Fusillade;
using Refit;
using Tobania.Xam.Tobit.Config;
using Xamarin.Forms;

namespace Tobania.Xam.Tobit.ServiceAgents.GitHub
{
	public class GitHubServiceAgent: IGitHubServiceAgent
	{
		private readonly Lazy<IGitHubApi> userInitiated;
		private readonly Lazy<IGitHubApi> background;
		private readonly Lazy<IGitHubApi> speculative;

		public GitHubServiceAgent()
		{
			Func<HttpMessageHandler, IGitHubApi> createClient = messageHandler =>
			{
				var client = new HttpClient(messageHandler)
				{
					BaseAddress = new Uri(ApiKeys.GitHubApi),
					Timeout = new TimeSpan(0, 2, 0)
				};
				return RestService.For<IGitHubApi>(client);
			};
			Func<string> createAuthHeader = () =>
			{
				return Application.Current.Properties[ApiKeys.AccessToken].ToString();
			};
			userInitiated = new Lazy<IGitHubApi>(() => createClient(new RateLimitedHttpMessageHandler(new ApiHandler(createAuthHeader), Priority.UserInitiated)));
			background = new Lazy<IGitHubApi>(() => createClient(new RateLimitedHttpMessageHandler(new ApiHandler(createAuthHeader), Priority.Background)));
			speculative = new Lazy<IGitHubApi>(() => createClient(new RateLimitedHttpMessageHandler(new ApiHandler(createAuthHeader), Priority.Speculative)));
		}

		public IGitHubApi UserInitiated => userInitiated.Value;
		public IGitHubApi Background => background.Value;
		public IGitHubApi Speculative => speculative.Value;
	}
}
