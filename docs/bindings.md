# Bindings

## Overview
The last thing we need to do is connect all the pieces we made. We'll create our ViewModels to connect our model with our views. There are a lot of MVVM frameworks out there you can use. Because this tutorial is not about MVVM we'll stay with our own basic implementation. Because it's basic it will be easy for you to swap to a particular framework. 

## Steps

### The BaseViewModel
We'll start with creating a BaseViewModel that will help us out with the implementation of INotifyPropertyCHanged

- Create a new C#-file "INotifyPropertyChanging" in the ViewModels folder of the Tobania.Xam.Tobit project

```C#
public interface INotifyPropertyChanging
{
    event EventHandler<PropertyChangingEventArgs> PropertyChanging;
}
```

- Create a new C#-file "BaseViewModel" in the ViewModels folder of the Tobania.Xam.Tobit project

```C#
public class BaseViewModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    private bool _isBusy;

    protected BaseViewModel()
    {
    }

    public event PropertyChangedEventHandler PropertyChanged = delegate { };
    public event EventHandler<PropertyChangingEventArgs> PropertyChanging = delegate { };

    public bool IsBusy
    {
        get { return _isBusy; }
        set { SetProperty(ref _isBusy, value); }
    }

    protected void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null,
			Action onChanged = null, Action<T> onChanging = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return;
        if (onChanging != null)
            onChanging(value);
        OnPropertyChanging(propertyName);

        backingStore = value;

        if (onChanged != null)
            onChanged();
        OnPropertyChanged(propertyName);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = null)
    {
        PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
    }
}
```

### The HomeViewModel
- Create a new C#-file "HomeViewModel" in the ViewModels folder
- Inherit form BaseViewModel
- We'll need a command for the login button of our HomePage and a command to navigate to our repositories

```C#
#region Commands
private ICommand loginCommand;
public ICommand LoginCommand
{
get { return loginCommand ?? (loginCommand = new Command(() => ExecuteLoginCommand())); }
}
private ICommand gitHubReposCommand;
public ICommand GitHubReposCommand
{
get { return gitHubReposCommand ?? (gitHubReposCommand = new Command(() => ExecuteGitHubReposCommand())); }
}
#endregion
```

- The ExecuteLoginCommand and the ExecuteGitHubReposCommand need to navigate to another page. The Pages however are in our UI project. Remember in MVVM you ViewModel may not know about your View. We can solve that problem via the MessagingService build into Xamarin.Forms
- Create a C#-file MessageKeys in the Config folder.

```C#
public static class MessageKeys
{
    public const string NavigateToLogin = "navigate_login";
	public const string NavigateToRepos = "navigate_repos";
}
```

- Implement the ExecuteLoginCommand and the ExecuteGitHubReposCommand

```C#
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
```

 - We shouldn't be able to execute the GitHubReposCommand when we're not authenticated. So add a method IsAuthenticated and add the check to our GitHubReposCommand
    
```C#
private ICommand gitHubReposCommand;
public ICommand GitHubReposCommand
{
    get { return gitHubReposCommand ?? (gitHubReposCommand = new Command(() => ExecuteGitHubReposCommand(),() => IsAuthenticated())); }
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
```

- Each time the HomePage is loaded, we should check again if the user is authenticated to enable/disable the butten. But, our viewmodel doesn't know when it must check. So create an Initialize method that triggers the GitHubReposCommand to check it's canExecute parameter

```C#
public void Initialize()
{
    ((Command)GitHubReposCommand).ChangeCanExecute();
}
```

### Bind the HomePage
Now we have our HomViewModel, we can bind the HomePage
- In HomePage.xaml bind the two buttons to the commands we created in the HomeViewModel

```html
<Button Text="Login" Command="{Binding LoginCommand}" />
<Button Text="GitHubRepos" Command="{Binding GitHubReposCommand}" />
```        

- In the HomePage.xaml.cs we need to set our BindingContext to an instance of our HomeViewModel

```C#
public HomePage()
{
    BindingContext = new HomeViewModel();
    InitializeComponent();
}
```

- You can add a private property to your page so you can alwasy call ViewModel (so you don't have to remember which viewmodel you are using in the rest of the code of your page). 

```C#
private HomeViewModel ViewModel
{
    get { return BindingContext as HomeViewModel; }
}
```

- Our HomePage needs to Initialize the ViewModel and listen to the MessagingCenter if we get a trigger to navigate (sent from the HomeViewModel)

```C#
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
```

- When you subscribe to events or messages on the MessagingCenter you should **ALWAYS** unsubscribe to avoid memory leaks

```C#
protected override void OnDisappearing()
{
    MessagingCenter.Unsubscribe<HomeViewModel>(this, MessageKeys.NavigateToLogin);
    MessagingCenter.Unsubscribe<HomeViewModel>(this, MessageKeys.NavigateToRepos);
    base.OnDisappearing();
}
```


### The ReposViewModel
In the ReposViewModel we'll finally make use of our GitHub service agent.

- Create a new C#-file ReposViewModel in th ViewModels folder.
- Inherit form BaseViewModel
- Create in the constructor an instance of GitHubService (for which you'll need to pass an instance of GitHubServiceAgent)

```C#
private IGitHubService gitHubService;
public ReposViewModel()
{
    IGitHubServiceAgent gitHubServiceAgent = new GitHubServiceAgent();
    gitHubService = new GitHubService(gitHubServiceAgent);
}
```

- Create a command to load the data

```C#
private ICommand loadDataCommand;
public ICommand LoadDataCommand
{
    get { return loadDataCommand ?? (loadDataCommand = new Command(async () => await ExecuteLoadDataCommand())); }
}
```

- Implement the ExecuteLoadDataCommand. 

```C#
private async Task ExecuteLoadDataCommand()
{
    GitHubRepos = new ObservableCollection<GitHubRepo>(await gitHubService.GetReposAsync());
}
```

- We also need a collection of all the GitHubRepo objects we receive from our cache or from GitHub. Because our View need to be able to bind to that colleciton we create a bindable property.

```C#
private ObservableCollection<GitHubRepo> gitHubRepos = new ObservableCollection<GitHubRepo>();
public ObservableCollection<GitHubRepo> GitHubRepos
{
    get { return gitHubRepos; }
    set { SetProperty(ref gitHubRepos, value); }
}
```

### The ReposPage
- In the ReposPage.xaml.cs you need to set the Bindingcontext to our ReposViewModel (just like we did with the HomePage)

```C#
public ReposPage()
{
    BindingContext = new ReposViewModel();
    InitializeComponent();
}

private ReposViewModel ViewModel
{
    get { return BindingContext as ReposViewModel; }
}

```

- When the page appears we want to tell the ViewModel to load the data. You should not load data in the constructor of your Page as it will slow down your app experience.

```C#
protected override void OnAppearing()
{
    base.OnAppearing();
    ViewModel.LoadDataCommand.Execute(null);
}
```

- In the ReposPage.xaml we need to bind our ListView to the list of GitHubRepo objects
```html
<ListView x:Name="ReposList" ItemsSource="{Binding GitHubRepos}">
```

- finally we want to bind the labels from our ReposCell to properties of a GitHubRepo. The bindings for the labels should already be in place in the ReposCell.xaml
```html
<Label x:Name="RepoName" Text="{Binding Name}" FontSize="Small" />
<Label x:Name="RepoFullName" Text="{Binding FullName}" FontSize="Micro" />
```