using System;
namespace Tobania.Xam.Tobit.ViewModels
{
	public interface INotifyPropertyChanging
	{
		event EventHandler<PropertyChangingEventArgs> PropertyChanging;
	}
}
