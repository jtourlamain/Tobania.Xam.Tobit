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
get { return loginCommand ?? (loginCommand = new Command(async () => await ExecuteLoginCommand())); }
}
private ICommand gitHubReposCommand;
public ICommand GitHubReposCommand
{
get { return gitHubReposCommand ?? (gitHubReposCommand = new Command(async () => await ExecuteGitHubReposCommand())); }
}
#endregion
```