# Create the API calls

## Overview
Mobile applications are more vulnerable for connectivity loss than desktop computers. You take your mobile phone with you when taking the bus, taking an elevator, while walking in the woods,... Sometimes your connection is very poor, sometimes you don't have any connection at all. Still, you want to give your users the best experience possible. In this section we'll have a look how you can implement your API calls, make sure you retry a request when something went wrong and cache the data so a user get a better overall experience.

## Remark
The NuGet packages for our PCL projects often contain just an abstraction. The real implementation is defined in the platform specific package. So it is important that you install the packages in the PCL as well in the platform specific project! 

## Steps

### Json.NET
- Install the NuGet package Newtonsoft.Json. 

### HTTPClient
- Install the NuGet package Microsoft.Net.http. This package only needs to be installed in the PCL "Tobania.Xam.Tobit".

### Refit
You can use the HtppClient for all your network calls, but Refit makes it really easy to implement API calls and it's easy to reuse code.
Info about Refit can be found on [https://github.com/paulcbetts/refit](https://github.com/paulcbetts/refit)

- Install the NuGet package Refit (version 2.4.1). There is a higher version available that makes use of the .NET standard. As XS doesn't fully support it, we'll use the previous version.
- In the Tobania.Xam.Tobit project create a new folder GitHub in the ServiceAgents folder.
- Refit allows us to define an interface that describes the API we'll be interacting with. Refit will do the hard work and make an implementation for us. So, create a new C#-file IGitHubApi in the folder you just created. 
- We want to call the repos of the logged in user. The interface looks like this:
```c#
[Headers("Authorization: Bearer", "Accept: application/vnd.github.v3+json", "User-Agent: Tobit")]
public interface IGitHubApi
{
    [Get("/user/repos")]
    Task<List<GitHubRepo>> GetReposAsync();
}
``` 
The top headers attribute contains the headers that will be send with each request. You notice that we'll use the authorization header, an accept header (cf github API documentation) and a user-agent (mandatory by GitHub)
The interface defines one method: GetReposAsync. This is the method we'll use in our code. The actual method is defined in the attribute above the method. You'll notice it's a get request with endpoint "/users/repos".

### The model
The GetReposAsync method returns a Task<List<GItHubRepo>>. We'll create that file and define which values from the response we want to work with.
- Create a C#-file GitHubRepo in the folder Models
- Add the following code to the file
```C#
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
``` 


### Fusillade
Now we can define all our calls in the IGitHubApi interface. The problem is that we don't make a distinction between calls initiated by a user and calls that our app makes in the background. If we can give priority to the calls that a user makes, the user will get the impression that the app repsonds quicker. We can solve this problem with Fusillad. The package comes also with other features like auto-deduplication of requests and request limiting.
Info about Fusillade can be found on [https://github.com/paulcbetts/Fusillade](https://github.com/paulcbetts/Fusillade)

- Install the NuGet package Refit (version 0.6.0).
- Create a new C#-file "IGitHubServiceAgent" in the folder ServiceAgents/GitHub.
- Define the 3 different ways to make calls via fussilade
```C#
public interface IGitHubServiceAgent
{
    IGitHubApi UserInitiated { get; }
    IGitHubApi Background { get; }
    IGitHubApi Speculative { get; }
}
```
- In the same folder create a C#-file "GitHubServiceAgent" that implements the "IGitHubServiceAgent" interface.
- In the constructor create a function that returns an instance of the IGitHubApi (created by Refit). We could just use the following code:
```C#
Func<IGitHubApi> createClient = () => 
{
    return RestService.For<IGitHubApi>("https://api.github.com");
}
```
There are some problems with this approach:
- By default Xamarin uses the Mono networking stack, which is ok, but the native networks stacks are faster. We'll use another NuGet package to solve this.
- We can't define a time-out on the calls 
- We can't manipulate the calls (like injecting the token on the Authorization header).
So, we'll change our function into:
```C#
Func<HttpMessageHandler, IGitHubApi> createClient = messageHandler =>
{
    var client = new HttpClient(messageHandler)
    {
        BaseAddress = new Uri(ApiKeys.GitHubApi),
        Timeout = new TimeSpan(0, 2, 0)
    };
    return RestService.For<IGitHubApi>(client);
};
```
- Create in the constructor a function that returns our accessToken:
```C#
Func<string> createAuthHeader = () =>
{
    return Application.Current.Properties[ApiKeys.AccessToken].ToString();
};
```
- The last thing we need to do is link the creation of the Refit services with Fusillade. Also in the constructor add:
```C#
userInitiated = new Lazy<IGitHubApi>(() => createClient(new RateLimitedHttpMessageHandler(new ApiHandler(createAuthHeader), Priority.UserInitiated)));
background = new Lazy<IGitHubApi>(() => createClient(new RateLimitedHttpMessageHandler(new ApiHandler(createAuthHeader), Priority.Background)));
speculative = new Lazy<IGitHubApi>(() => createClient(new RateLimitedHttpMessageHandler(new ApiHandler(createAuthHeader), Priority.Speculative)));
```
Your GitHubServiceAgent constructor should now look like:
```C#
public GitHubServiceAgent()
{
    Func<HttpMessageHandler, IGitHubApi> createClient = messageHandler =>
    {
        var client = new HttpClient(messageHandler)
        {
            BaseAddress = new Uri(ApiKeys.GitHubApi),
            Timeout = new TimeSpan(0, 2, 0)
        };
        return RestService.For<IGitHubApi>(client);
    };
    Func<string> createAuthHeader = () =>
    {
        return Application.Current.Properties[ApiKeys.AccessToken].ToString(); 
    };
    userInitiated = new Lazy<IGitHubApi>(() => createClient(new RateLimitedHttpMessageHandler(new ApiHandler(createAuthHeader), Priority.UserInitiated)));
    background = new Lazy<IGitHubApi>(() => createClient(new RateLimitedHttpMessageHandler(new ApiHandler(createAuthHeader), Priority.Background)));
    speculative = new Lazy<IGitHubApi>(() => createClient(new RateLimitedHttpMessageHandler(new ApiHandler(createAuthHeader), Priority.Speculative)));
}
```

### ModernHttpClient
As mentioned in the Fusillade section Xamarin uses the Mono Network stack. We want to use the faster native network stacks. For that reason we use ModernHttpClient.
Info about Modernhttpclient can be found on [https://github.com/paulcbetts/ModernHttpClient](https://github.com/paulcbetts/ModernHttpClient) 

- Install the NuGet package Modernhttpclient (version 2.4.2).
- Our code still gives an error on the ApiHandler. Create a new folder Helpers in the ServiceAgents folder and add a C#-file with name ApiHelper.
- Your ApiHandler class must inherit from NativeMessageHandler (comes with the ModernHttpClient package)
- In the constructor we accept the func to get the token
```C#
private readonly Func<string> getToken;

public ApiHandler(Func<string> getToken)
{
    this.getToken = getToken;
}
```
- Override the SendAsync method and check if you need to add the token before the request is actually made
```C#
protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
{
    var auth = request.Headers.Authorization;
    if (auth != null)
    {
        var token = getToken();//.ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, token);
    }
    return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
}
```

### Service calls
After some plumbing code we can start with defining our service calls.


- Create a new C#-file "IGitHubService" in the folder ServiceAgents/GitHub
```C#
public interface IGitHubService
{
    Task<List<GitHubRepo>> GetReposAsync();
}
```
- Now add another file in the same folder with name "GitHubService" and inherit from the "IGitHubService" interface.
- Make sure the constructor accepts an "IGitHubServiceAgent" so you can make use of the priority features that fussilade gives us.
```C#
private IGitHubServiceAgent serviceAgent;

public GitHubService(IGitHubServiceAgent serviceAgent)
{
    this.serviceAgent = serviceAgent;
}

public async Task<List<GitHubRepo>> GetReposAsync()
{
    return await serviceAgent.UserInitiated.GetReposAsync().ConfigureAwait(false);
}
```

### Check connectivity
Our service calls might work now, but it's useless to start a call when your device is not connected.
Info about the connectivity plugin can be found on [https://github.com/jamesmontemagno/ConnectivityPlugin](https://github.com/jamesmontemagno/ConnectivityPlugin)

- Install the NuGet package xam.plugin.connectivity (2.2.12). 
- When installing the NuGet package you get a readme file. You should read those files as they contain important notes from the developer. The file mentions that you should give your android app more premissions. So, in the Android project open up the AndroidManifest.xml in the properties folder. Add the following permissions between the opening and closing manifest tag:
    - ```<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />```
	- ```<uses-permission android:name="android.permission.ACCESS_WIFI_STATE" />```
	- ```<uses-permission android:name="android.permission.INTERNET" />```
- Use the connectivity plugin to verify if you have a connection before executing the API call
```C#
public async Task<List<GitHubRepo>> GetReposAsync()
{
    List<GitHubRepo> result = new List<GitHubRepo>();
    if (CrossConnectivity.Current.IsConnected)
    {
        result = await serviceAgent.UserInitiated.GetReposAsync().ConfigureAwait(false);	
    }
    return result;
}
```

### Retry policy
Because the connectivity suddenly get poor when the API call is executed, you should retry your calls. We can create a retry policy with Polly.
Info about Polly can be found on [https://github.com/App-vNext/Polly](https://github.com/App-vNext/Polly)

- Install the NuGet package Polly (4.3.0)
- Define a policy in the constructor of the GitHubService class
```C#
private IGitHubServiceAgent serviceAgent;
private Policy policy;

public GitHubService(IGitHubServiceAgent serviceAgent)
{
    this.serviceAgent = serviceAgent;
    policy = Policy
    .Handle<WebException>()
    .WaitAndRetryAsync(5, retryAttempt =>
    TimeSpan.FromSeconds(retryAttempt)
    );
}
```
- Define that your service call must use the policy
```C#
public async Task<List<GitHubRepo>> GetReposAsync()
{
    List<GitHubRepo> result = new List<GitHubRepo>();
    if (CrossConnectivity.Current.IsConnected)
    {
        try
        {
            result = await policy.ExecuteAsync(async () => await serviceAgent.UserInitiated.GetReposAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var m = ex.Message;
            // if you still get an error, log the exception
        }
    }
    return result;
}
```

### Caching
You should avoid requesting the data all the time by caching it. Your users will be grateful that you don't spoil their data plan, they will have the impression that your app runs faster and less calls to your backend means less costs.
For caching we'll make use of Akavache. Behind the scenes akavache makes use of SQLite. Optimizing your code to use SQLite is hard. The Akavache library contains all the hard work, so our job gets a lot easier.

- Install the NuGet package Akavache (4.1.2) (you also need to install this package in the UI project)
- In the App.cs constructor set the akavache application name
```C#
BlobCache.ApplicationName = AppKeys.ApplicationName;
```
- Create a new C#-file "AppKeys.cs" in the config folder of the project Tobania.Xam.Tobit
```C#
public static class AppKeys
{
    public const string ApplicationName = "EntDemo";
}
```
- Now we'll change our GitHubService class to use the cached version of the data and retrieve the real data async if the data in the cache is older than 5 seconds. First in the GitHubService class change the methodname "GetReposAsync" to "GetRemoteReposAsync"
- Add a GetReposAsync method with the same signature as before and request the data from the cache
```C#
public async Task<List<GitHubRepo>> GetReposAsync()
		{
			var cachedRepos = BlobCache.LocalMachine.GetAndFetchLatest<List<GitHubRepo>>(nameof(GitHubRepo), 
			                                                           () => GetRemoteReposAsync(), 
			                                                           offset =>
												  					   {				
																	  		TimeSpan elapsed = DateTimeOffset.UtcNow - offset;
																	  		return elapsed > new TimeSpan(hours: 0, minutes: 0, seconds: 3);
																		});
			return await cachedRepos.FirstOrDefaultAsync();
}
```

## Recap
We now have a decent start to get the data out of our cache, call our API if the cache is expired, only make the call if we are connected, retry if necassary and in a prioritized way. And on top of that we're making use of the native network stack.
