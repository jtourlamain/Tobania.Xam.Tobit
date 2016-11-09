using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
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
