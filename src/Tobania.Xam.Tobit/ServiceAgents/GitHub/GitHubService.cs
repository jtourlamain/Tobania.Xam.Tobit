using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using Plugin.Connectivity;
using Polly;
using Tobania.Xam.Tobit.Models;

namespace Tobania.Xam.Tobit.ServiceAgents.GitHub
{
	public class GitHubService: IGitHubService
	{
		private IGitHubServiceAgent serviceAgent;
		private Policy policy;


		public GitHubService(IGitHubServiceAgent serviceAgent)
		{
			this.serviceAgent = serviceAgent;
			policy = Policy
			  .Handle<WebException>()
			  .WaitAndRetryAsync(5, retryAttempt =>
				TimeSpan.FromSeconds(retryAttempt)
  				);
		}


		public async Task<List<GitHubRepo>> GetReposAsync()
		{
			var cachedRepos = BlobCache.LocalMachine.GetAndFetchLatest<List<GitHubRepo>>(nameof(GitHubRepo),
																	   () => GetRemoteReposAsync(),
																	   offset =>
																		 {
																			 TimeSpan elapsed = DateTimeOffset.UtcNow - offset;
																			 return elapsed > new TimeSpan(hours: 0, minutes: 0, seconds: 3);
																		 });
			return await cachedRepos.FirstOrDefaultAsync();
		}

		public async Task<List<GitHubRepo>> GetRemoteReposAsync()
		{



			List<GitHubRepo> result = new List<GitHubRepo>();
			if (CrossConnectivity.Current.IsConnected)
			{
				try
				{
					result = await policy.ExecuteAsync(async () => await serviceAgent.UserInitiated.GetReposAsync().ConfigureAwait(false)).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					var m = ex.Message;
					// log the exception
				}
			}
			return result;
		}
	}
}
