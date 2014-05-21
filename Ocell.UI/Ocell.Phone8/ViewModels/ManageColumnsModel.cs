
using AncoraMVVM.Base;
using Ocell.Library;
using Ocell.Library.Twitter;
using PropertyChanged;
using System.Collections.Specialized;
using System.Diagnostics;
namespace Ocell.ViewModels
{
    [ImplementPropertyChanged]
    public class ManageColumnsModel : ExtendedViewModelBase
    {
        public SafeObservable<TwitterResource> Columns { get; set; }
        public int SelectedColumn { get; set; }
        public DelegateCommand DeleteColumnCommand { get; set; }

        ObservableCollectionReplayer replayer = new ObservableCollectionReplayer();

        public ManageColumnsModel()
        {
            Columns = new SafeObservable<TwitterResource>(Config.Columns.Value);

            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "SelectedColumn" && SelectedColumn != -1)
                    SelectedColumn = -1;
            };

            replayer.ReplayTo(Columns, Config.Columns.Value);

            Config.Columns.Value.CollectionChanged += (sender, e) => Config.SaveColumns();
            DeleteColumnCommand = new DelegateCommand(DeleteColumn);
        }

        private void OnColumnsChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("{0}");
        }

        public void DeleteColumn(object parameter)
        {
            var column = parameter as TwitterResource;

            if (column != null)
                Columns.Remove(column);
        }
    }
}
