//      *********    NO MODIFIQUE ESTE ARCHIVO     *********
//      Este archivo se regenera mediante una herramienta de diseño.
//       Si realiza cambios en este archivo, puede causar errores.
namespace Expression.Blend.SampleData.FiltersSampleData
{
	using System; 
	using System.ComponentModel;

// Para reducir de forma significativa la superficie de los datos de ejemplo en la aplicación de producción, puede establecer
// la constante de compilación condicional DISABLE_SAMPLE_DATA y deshabilitar los datos de ejemplo en tiempo de ejecución.
#if DISABLE_SAMPLE_DATA
	internal class FiltersSampleData { }
#else

	public class FiltersSampleData : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public FiltersSampleData()
		{
			try
			{
				Uri resourceUri = new Uri("/Ocell;component/SampleData/FiltersSampleData/FiltersSampleData.xaml", UriKind.RelativeOrAbsolute);
				System.Windows.Application.LoadComponent(this, resourceUri);
			}
			catch
			{
			}
		}

		private Filters _Filters = new Filters();

		public Filters Filters
		{
			get
			{
				return this._Filters;
			}
		}
	}

	public class FiltersItem : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private string _Duration = string.Empty;

		public string Duration
		{
			get
			{
				return this._Duration;
			}

			set
			{
				if (this._Duration != value)
				{
					this._Duration = value;
					this.OnPropertyChanged("Duration");
				}
			}
		}

		private string _Filter = string.Empty;

		public string Filter
		{
			get
			{
				return this._Filter;
			}

			set
			{
				if (this._Filter != value)
				{
					this._Filter = value;
					this.OnPropertyChanged("Filter");
				}
			}
		}

		private string _IsValidUntil = string.Empty;

		public string IsValidUntil
		{
			get
			{
				return this._IsValidUntil;
			}

			set
			{
				if (this._IsValidUntil != value)
				{
					this._IsValidUntil = value;
					this.OnPropertyChanged("IsValidUntil");
				}
			}
		}
	}

	public class Filters : System.Collections.ObjectModel.ObservableCollection<FiltersItem>
	{ 
	}
#endif
}
