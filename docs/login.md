# Login

## Overview
For the authorization flow, the login makes use of a webpage. To open up an webview and return into your Xamarin.Forms flow you'll need a custom renderer because each platform handles that flow in a different way.

## Steps

### The shared login page
First of all we need a page to navigate to that our Xamarin Forms understands.

- In the Tobania.Xam.Tobit.UI project create a new folder "Login" in the Pages folder.
- In the Login folder create a new XAML-ContentPage "LoginPage"
- Change the namespace in the same way you did in the layout section of this tutorial.

That's all we need to do in the shared code.

### Login Renderer for iOS
- In the iOS project, right click on Components and add the component Xamarin.authorization
- In the renderers folder create a new C#-file "LoginPageRenderer"
- Your class should inherit from PageRenderer (found in the namespace Xamarin.Forms.Platform.iOS)
- Override the ViewDidAppear method. There seems to be a bug in the iOS version that reopens your login window each time your login was successfull. You can avoid the loop by setting the MainPage of your application

```C#
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
```

- The last thing we need to do is tell Xamarin.Forms that every time we ask a LoginPage, we wat it to take our LoginPageRenderer. In your LoginPageRenderer *above* your namespace declaration place the following

```C#
[assembly: ExportRenderer(typeof(LoginPage), typeof(LoginPageRenderer))]
```

### Login Renderer for Android
- In the Android project, right click on Components and add the component Xamarin.authorization
- In the renderers folder create a new C#-file "LoginPageRenderer"
- Your class should inherit from PageRenderer (found in the namespace Xamarin.Forms.Platform.Android)

```C#
public class LoginPageRenderer: PageRenderer
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
```

- The last thing we need to do is tell Xamarin.Forms that every time we ask a LoginPage, we wat it to take our LoginPageRenderer. In your LoginPageRenderer *above* your namespace declaration place the following

```C#
[assembly: ExportRenderer(typeof(LoginPage), typeof(LoginPageRenderer))]
```
