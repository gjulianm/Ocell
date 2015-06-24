//      *********    NO MODIFIQUE ESTE ARCHIVO     *********
//      Este archivo se regenera mediante una herramienta de diseño.
//       Si realiza cambios en este archivo, puede causar errores.
namespace Expression.Blend.SampleData.TwitterResourceData
{
	using System; 
	using System.ComponentModel;

// Para reducir de forma significativa la superficie de los datos de ejemplo en la aplicación de producción, puede establecer
// la constante de compilación condicional DISABLE_SAMPLE_DATA y deshabilitar los datos de ejemplo en tiempo de ejecución.
#if DISABLE_SAMPLE_DATA
	internal class TwitterResourceData { }
#else

	public class TwitterResourceData : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public TwitterResourceData()
		{
			try
			{
				Uri resourceUri = new Uri("/Ocell;component/SampleData/TwitterResourceData/TwitterResourceData.xaml", UriKind.RelativeOrAbsolute);
				System.Windows.Application.LoadComponent(this, resourceUri);
			}
			catch
			{
			}
		}

		private Lists _Lists = new Lists();

		public Lists Lists
		{
			get
			{
				return this._Lists;
			}
		}
	}

	public class ListsItem : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private string _String = string.Empty;

		public string String
		{
			get
			{
				return this._String;
			}

			set
			{
				if (this._String != value)
				{
					this._String = value;
					this.OnPropertyChanged("String");
				}
			}
		}
	}

	public class Lists : System.Collections.ObjectModel.ObservableCollection<ListsItem>
	{ 
	}
#endif
}
