using System;
using System.Collections.Generic;
using Tobania.Xam.Tobit.Config;
using Tobania.Xam.Tobit.ViewModels;
using Xamarin.Forms;

namespace Tobania.Xam.Tobit.UI
{
	public partial class HomePage : ContentPage
	{
		public HomePage()
		{
			BindingContext = new HomeViewModel();
			InitializeComponent();
		}

		private HomeViewModel ViewModel
		{
			get { return BindingContext as HomeViewModel; }
		}

		protected override void OnAppearing()
		{
			ViewModel.Initialize();
			MessagingCenter.Subscribe<HomeViewModel>(this, MessageKeys.NavigateToLogin, async _ =>
			  {
				  await Navigation.PushModalAsync(new LoginPage());
			  });
			MessagingCenter.Subscribe<HomeViewModel>(this, MessageKeys.NavigateToRepos, async _ =>
			  {
				  await Navigation.PushModalAsync(new ReposPage());
			  });
			base.OnAppearing();
		}

		protected override void OnDisappearing()
		{
			MessagingCenter.Unsubscribe<HomeViewModel>(this, MessageKeys.NavigateToLogin);
			MessagingCenter.Unsubscribe<HomeViewModel>(this, MessageKeys.NavigateToRepos);
			base.OnDisappearing();
		}
	}
}
