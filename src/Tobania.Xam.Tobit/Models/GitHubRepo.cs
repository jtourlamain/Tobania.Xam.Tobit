using System;
using Newtonsoft.Json;

namespace Tobania.Xam.Tobit.Models
{
	public class GitHubRepo
	{
		public int Id { get; set; }
		public string Name { get; set; }
		[JsonProperty("full_name")]
		public string FullName { get; set; }
		public string Url { get; set; }
		[JsonProperty("html_url")]
		public string HtmlUrl { get; set; }
	}
}
