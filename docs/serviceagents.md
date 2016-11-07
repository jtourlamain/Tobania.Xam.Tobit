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
Info about Refit can be found on [https://github.com/paulcbetts/Fusillade](https://github.com/paulcbetts/Fusillade)

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
As mentioned in the Fusillade section Xamarin uses the Mono Network stack. We want to use the faster native network stacks. For that reason we use ModernHttpClient
- Install the NuGet package Modernhttpclient (version 2.4.2).




