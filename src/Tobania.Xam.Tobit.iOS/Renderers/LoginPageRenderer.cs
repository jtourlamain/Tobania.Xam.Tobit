using System;
using Tobania.Xam.Tobit.Config;
using Tobania.Xam.Tobit.iOS.Renderers;
using Tobania.Xam.Tobit.UI;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(LoginPage), typeof(LoginPageRenderer))]
namespace Tobania.Xam.Tobit.iOS.Renderers
{
	public class LoginPageRenderer: PageRenderer
	{
		public override async void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

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
				DismissViewController(true, null);

				if (eventArgs.IsAuthenticated)
				{
					App.Current.Properties[ApiKeys.AccessToken] = eventArgs.Account.Properties[ApiKeys.AccessToken].ToString();
					App.Current.MainPage = new NavigationPage(new HomePage());
				}
				else {
					App.Current.Properties[ApiKeys.AccessToken] = "";
					App.Current.MainPage = new NavigationPage(new HomePage());
				}
			};
			PresentViewController(auth.GetUI(), true, null);
		}

	}
}
