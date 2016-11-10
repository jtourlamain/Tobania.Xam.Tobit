using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Tobania.Xam.Tobit.Models;
using Tobania.Xam.Tobit.ServiceAgents.GitHub;
using Xamarin.Forms;

namespace Tobania.Xam.Tobit.ViewModels
{
	public class ReposViewModel : BaseViewModel
	{
		private IGitHubService gitHubService;
		public ReposViewModel()
		{
			IGitHubServiceAgent gitHubServiceAgent = new GitHubServiceAgent();
			gitHubService = new GitHubService(gitHubServiceAgent);
		}

		#region Bindable Properties
		private ObservableCollection<GitHubRepo> gitHubRepos = new ObservableCollection<GitHubRepo>();
		public ObservableCollection<GitHubRepo> GitHubRepos
		{
			get { return gitHubRepos; }
			set { SetProperty(ref gitHubRepos, value); }
		}
		#endregion

		#region Commands
		private ICommand loadDataCommand;
		public ICommand LoadDataCommand
		{
			get { return loadDataCommand ?? (loadDataCommand = new Command(async () => await ExecuteLoadDataCommand())); }
		}
		#endregion

		private async Task ExecuteLoadDataCommand()
		{
			GitHubRepos = new ObservableCollection<GitHubRepo>(await gitHubService.GetReposAsync());
		}
	}
}
