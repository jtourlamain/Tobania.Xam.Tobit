using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tobania.Xam.Tobit.Models;

namespace Tobania.Xam.Tobit.ServiceAgents.GitHub
{
	public interface IGitHubService
	{
		Task<List<GitHubRepo>> GetReposAsync();
	}
}
