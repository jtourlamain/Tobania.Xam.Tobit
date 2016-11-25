using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Tobania.Xam.Tobit.Models;

namespace Tobania.Xam.Tobit.ServiceAgents.GitHub
{
	[Headers("Authorization: Bearer", 
	         "Accept: application/vnd.github.v3+json", 
	         "User-Agent: Tobit")]
	public interface IGitHubApi
	{
		[Get("/user/repos")]
		Task<List<GitHubRepo>> GetReposAsync();
	}
}
