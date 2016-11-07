using System;
namespace Tobania.Xam.Tobit.ServiceAgents.GitHub
{
	public interface IGitHubServiceAgent
	{
		IGitHubApi UserInitiated { get; }
		IGitHubApi Background { get; }
		IGitHubApi Speculative { get; }
	}
}
