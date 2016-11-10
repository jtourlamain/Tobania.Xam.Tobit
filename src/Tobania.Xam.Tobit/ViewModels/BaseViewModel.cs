using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Tobania.Xam.Tobit.ViewModels
{
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
}
