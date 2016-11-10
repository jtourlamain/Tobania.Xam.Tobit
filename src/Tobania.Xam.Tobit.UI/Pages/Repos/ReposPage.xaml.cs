using System;
using System.Collections.Generic;
using Tobania.Xam.Tobit.ViewModels;
using Xamarin.Forms;

namespace Tobania.Xam.Tobit.UI
{
	public partial class ReposPage : ContentPage
	{
		public ReposPage()
		{
			BindingContext = new ReposViewModel();
			InitializeComponent();
		}

		private ReposViewModel ViewModel
		{
			get { return BindingContext as ReposViewModel; }
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			ViewModel.LoadDataCommand.Execute(null);
		}
	}
}
