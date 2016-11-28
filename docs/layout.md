# Some basic layout

## Overview
Because this tutorial isn't about layout we'll implement just a basic layout just to get us going.
Our Layout will contain 3 pages:
- A homepage. On this page we can navigate to the login page or the overview of our repositories.
- A loginpage. This page is used for the authorization flow.
- A repository page. This page is used to show us our GitHub repositories.

## Steps
Almost everything we need for layout will be placed into the "Tobania.Xam.Tobit.UI" project.
You already crated a Pages folder within that project. 

### HomePage
- Create a Home folder in the Pages folder. Add a new Forms ContentPage XAML and name it "HomePage". Depending on your project settings the page will have the namespace "Tobania.Xam.Tobit.UI.Pages.Home". It's ok if you want to work like that, but you might end up with adding a log of namespaces. As we call our pages xxPage and our cells xxCell it's obvious enough to work with. So you can change the namespace of the xaml file and the xaml.cs to "Tobania.Xam.Tobit.UI".
- We'll use the HomePage as our first page. So in the App.xaml.cs file you need to set the MainPage to a new instance of the HomePage. We want to be able to navigate back and forwards to our repositories later on, so wrap the homepage into an navigation page.

```
public App()
{
    InitializeComponent();
    MainPage = new NavigationPage(new HomePage());
}
```

- add a title to your homepage. It becomes available on the top of our page.

```
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms" 
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml" 
    x:Class="Tobania.Xam.Tobit.UI.HomePage"
    Title="Home">
    <ContentPage.Content>
    ...
```

- add a label and two buttons in the center of the page. As the content of a page can only contain 1 root view, you'll need to wrap the 3 view into a stacklayout.

```
<StackLayout VerticalOptions="Center" HorizontalOptions="Center">
    <Label Text="Tobit demo" />
    <Button Text="Login" />
    <Button Text="GitHubRepos" />
</StackLayout>
```

### Repos
- Create a Repos folder in the Pages folder. Add a new Forms ContentPage XAML and name it "ReposPage". Change the namespace in the same way you did for the HomePage.
- Add a ListView with name ReposList to the content of the page. We need to define the layout of the listView via a Cell. Xamarin.Forms provides some default cells. More info on [https://developer.xamarin.com/guides/xamarin-forms/user-interface/listview/](https://developer.xamarin.com/guides/xamarin-forms/user-interface/listview/). In this demo I'll show you how to get organized with a custom cell (although in this simple demo you could also use a TextCell)

```
<ContentPage.Content>
    <ListView x:Name="ReposList">
        <ListView.ItemTemplate>
            <DataTemplate>
                <ViewCell>
                    <ui:ReposCell/>
                </ViewCell>
            </DataTemplate>
        </ListView.ItemTemplate>
    </ListView>		
</ContentPage.Content>
```

- For our custom cell we used ui:ReposCell. Our ReposCell will be a custom view. We'll place it in another file so you can reuse the layout of the cell in other pages. You XAML doesn't know about the ui element, so we'll need to add an additional namespace. We only need to add 1 namespace because all our UI views (pages, cells,...) are placed in the same namespace. 

```
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms" 
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml" 
    xmlns:ui="clr-namespace:Tobania.Xam.Tobit.UI;assembly=Tobania.Xam.Tobit.UI" 
    x:Class="Tobania.Xam.Tobit.UI.ReposPage">
```

### ReposCell
- Create a new ContentView XAML file "ReposCell" in the Cells folder. Change the namespace in the same way you did for the HomePage.
- We'll use a stacklayout to show the name of the repository and the full name. We'll bind to those properties and we make the font sizes a bit smaller. Like in html it's not a good idea to change font sizes in each xaml file. You should use the Application.Resources section in the App.xaml file (or create a static file with resources). Again, focus here is not on layout, so we just change the font size in our ReposCell.xaml file. The wrapping option is added for performance reasons. No wrapping is faster than letting XForms calculate where to wrap and adapt the height of your row.

```
<StackLayout Margin="20,0,20,0">
    <Label x:Name="RepoName" Text="{Binding Name}" FontSize="Small" LineBreakMode="NoWrap" />
    <Label x:Name="RepoFullName" Text="{Binding FullName}" FontSize="Micro" LineBreakMode="NoWrap" />
</StackLayout>
```


### loginpage
- Create a Login folder in the Pages folder. Add a new Forms ContentPage XAML and name it "LoginPage". Change the namespace in the same way you did for the HomePage.
- We don't need to make any more changes to the LoginPage as the login page interacts differently on each platform. We'll need to make use of a custom renderer in order to make it work. Custom renderers are the only layout exceptions that don't go into your UI project, but in the renderers folder of your iOS or Android project.




