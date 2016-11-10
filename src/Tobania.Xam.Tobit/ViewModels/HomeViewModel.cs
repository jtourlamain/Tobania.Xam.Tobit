using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Tobania.Xam.Tobit.Config;
using Xamarin.Forms;

namespace Tobania.Xam.Tobit.ViewModels
{
	public class HomeViewModel: BaseViewModel
	{

		public void Initialize()
		{
			((Command)GitHubReposCommand).ChangeCanExecute();
		}

		#region Commands
		private ICommand loginCommand;
		public ICommand LoginCommand
		{
			get { return loginCommand ?? (loginCommand = new Command(() => ExecuteLoginCommand())); }
		}
		private ICommand gitHubReposCommand;
		public ICommand GitHubReposCommand
		{
			get { return gitHubReposCommand ?? (gitHubReposCommand = new Command(() => ExecuteGitHubReposCommand(),() => IsAuthenticated())); }
		}
		#endregion

		private void  ExecuteLoginCommand()
		{
			if (IsBusy)
				return;
			MessagingCenter.Send<HomeViewModel>(this, MessageKeys.NavigateToLogin);
			IsBusy = false;
		}

		private void ExecuteGitHubReposCommand()
		{
			if (IsBusy)
				return;
			IsBusy = true;
			MessagingCenter.Send<HomeViewModel>(this, MessageKeys.NavigateToRepos);
			IsBusy = false;
		}

		private bool IsAuthenticated()
		{
			try
			{
				if (!string.IsNullOrEmpty(Application.Current.Properties[ApiKeys.AccessToken].ToString()))
				{
					return true;
				}
				return false;
			}
			catch (KeyNotFoundException)
			{
				return false;
			}
		}

	}
}
