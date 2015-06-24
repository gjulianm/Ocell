//      *********    NO MODIFIQUE ESTE ARCHIVO     *********
//      Este archivo se regenera mediante una herramienta de diseño.
//       Si realiza cambios en este archivo, puede causar errores.
namespace Expression.Blend.SampleData.ListsSampleDataSource
{
	using System; 
	using System.ComponentModel;

// Para reducir de forma significativa la superficie de los datos de ejemplo en la aplicación de producción, puede establecer
// la constante de compilación condicional DISABLE_SAMPLE_DATA y deshabilitar los datos de ejemplo en tiempo de ejecución.
#if DISABLE_SAMPLE_DATA
	internal class ListsSampleDataSource { }
#else

	public class ListsSampleDataSource : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public ListsSampleDataSource()
		{
			try
			{
				Uri resourceUri = new Uri("/Ocell;component/SampleData/ListsSampleDataSource/ListsSampleDataSource.xaml", UriKind.RelativeOrAbsolute);
				System.Windows.Application.LoadComponent(this, resourceUri);
			}
			catch
			{
			}
		}

		private ListUsers _ListUsers = new ListUsers();

		public ListUsers ListUsers
		{
			get
			{
				return this._ListUsers;
			}
		}

		private string _ListName = string.Empty;

		public string ListName
		{
			get
			{
				return this._ListName;
			}

			set
			{
				if (this._ListName != value)
				{
					this._ListName = value;
					this.OnPropertyChanged("ListName");
				}
			}
		}

		private AddUsers _AddUsers = new AddUsers();

		public AddUsers AddUsers
		{
			get
			{
				return this._AddUsers;
			}
		}

		private bool _CanFindMoreUsers = false;

		public bool CanFindMoreUsers
		{
			get
			{
				return this._CanFindMoreUsers;
			}

			set
			{
				if (this._CanFindMoreUsers != value)
				{
					this._CanFindMoreUsers = value;
					this.OnPropertyChanged("CanFindMoreUsers");
				}
			}
		}
	}

	public class ListUsersItem : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private System.Windows.Media.ImageSource _ProfileImageUrl = null;

		public System.Windows.Media.ImageSource ProfileImageUrl
		{
			get
			{
				return this._ProfileImageUrl;
			}

			set
			{
				if (this._ProfileImageUrl != value)
				{
					this._ProfileImageUrl = value;
					this.OnPropertyChanged("ProfileImageUrl");
				}
			}
		}

		private string _ScreenName = string.Empty;

		public string ScreenName
		{
			get
			{
				return this._ScreenName;
			}

			set
			{
				if (this._ScreenName != value)
				{
					this._ScreenName = value;
					this.OnPropertyChanged("ScreenName");
				}
			}
		}

		private string _Name = string.Empty;

		public string Name
		{
			get
			{
				return this._Name;
			}

			set
			{
				if (this._Name != value)
				{
					this._Name = value;
					this.OnPropertyChanged("Name");
				}
			}
		}
	}

	public class ListUsers : System.Collections.ObjectModel.ObservableCollection<ListUsersItem>
	{ 
	}

	public class AddUsersItem : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private string _Name = string.Empty;

		public string Name
		{
			get
			{
				return this._Name;
			}

			set
			{
				if (this._Name != value)
				{
					this._Name = value;
					this.OnPropertyChanged("Name");
				}
			}
		}

		private string _ScreenName = string.Empty;

		public string ScreenName
		{
			get
			{
				return this._ScreenName;
			}

			set
			{
				if (this._ScreenName != value)
				{
					this._ScreenName = value;
					this.OnPropertyChanged("ScreenName");
				}
			}
		}

		private System.Windows.Media.ImageSource _ProfileImageUrl = null;

		public System.Windows.Media.ImageSource ProfileImageUrl
		{
			get
			{
				return this._ProfileImageUrl;
			}

			set
			{
				if (this._ProfileImageUrl != value)
				{
					this._ProfileImageUrl = value;
					this.OnPropertyChanged("ProfileImageUrl");
				}
			}
		}
	}

	public class AddUsers : System.Collections.ObjectModel.ObservableCollection<AddUsersItem>
	{ 
	}
#endif
}
