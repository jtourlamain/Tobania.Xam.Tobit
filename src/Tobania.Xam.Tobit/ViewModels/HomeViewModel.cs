using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Tobania.Xam.Tobit.ViewModels
{
	public class HomeViewModel: BaseViewModel
	{
		#region Commands
		private ICommand loginCommand;
		public ICommand LoginCommand
		{
			get { return loginCommand ?? (loginCommand = new Command(async () => await ExecuteLoginCommand())); }
		}
		private ICommand gitHubReposCommand;
		public ICommand GitHubReposCommand
		{
			get { return gitHubReposCommand ?? (gitHubReposCommand = new Command(async () => await ExecuteGitHubReposCommand())); }
		}
		#endregion

		private async Task ExecuteGitHubReposCommand()
		{
			if (IsBusy)
				return;
			IsBusy = true;
			MessagingCenter.Send<HomeViewModel>(this, MessageKeys.NavigateToRepos);
			IsBusy = false;
		}
	}
}
