using System;
using Tobania.Xam.Tobit.UI;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Tobania.Xam.Tobit.Droid.Renderers;
using Android.App;
using Tobania.Xam.Tobit.Config;

[assembly: ExportRenderer(typeof(LoginPage), typeof(LoginPageRenderer))]
namespace Tobania.Xam.Tobit.Droid.Renderers
{
	public class LoginPageRenderer : PageRenderer
	{
		bool done = false;

		protected override async void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			base.OnElementChanged(e);
			if (!done)
			{
				var activity = this.Context as Activity;
				var auth = new OAuth2Authenticator(
					clientId: ApiKeys.ClientId,
					clientSecret: ApiKeys.ClientSecret,
					scope: ApiKeys.Scope,
					authorizeUrl: new Uri(ApiKeys.AuthorizeUrl),
					redirectUrl: new Uri(ApiKeys.RedirectUrl),
					accessTokenUrl: new Uri(ApiKeys.AccessTokenUrl)
				);

				auth.Completed += (sender, eventArgs) =>
				{
					if (eventArgs.IsAuthenticated)
					{
						App.Current.Properties[ApiKeys.AccessToken] = eventArgs.Account.Properties[ApiKeys.AccessToken].ToString();
						App.Current.MainPage.Navigation.PopModalAsync();
					}
					else
					{
						App.Current.Properties[ApiKeys.AccessToken] = "";
						App.Current.MainPage.Navigation.PopModalAsync();
					}
				};
				activity.StartActivity(auth.GetUI(activity));
				done = true;
			}
		}
	}
}
