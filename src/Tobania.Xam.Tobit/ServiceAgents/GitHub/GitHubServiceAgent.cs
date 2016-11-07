using System;
namespace Tobania.Xam.Tobit.ServiceAgents.GitHub
{
	public class GitHubServiceAgent: IGitHubServiceAgent
	{
		private readonly Lazy<IGitHubApi> userInitiated;
		private readonly Lazy<IGitHubApi> background;
		private readonly Lazy<IGitHubApi> speculative;

		public GitHubServiceAgent()
		{
		}

		public IGitHubApi UserInitiated => userInitiated.Value;
		public IGitHubApi Background => background.Value;
		public IGitHubApi Speculative => speculative.Value;
	}
}
