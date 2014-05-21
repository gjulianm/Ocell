using AncoraMVVM.Base;
using Ocell.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using PropertyChanged;

namespace Ocell.Pages.Search
{
    [ImplementPropertyChanged]
    public class ResourceViewModel : ExtendedViewModelBase
    {
        public string PageTitle { get; set; }
        public TweetLoader Loader { get; set; }
        public ExtendedListBox Listbox { get; set; }
        public TwitterResource Resource { get; set; }

        public DelegateCommand AddCommand { get; set; }

        public ResourceViewModel()
        {
            Resource = ReceiveMessage<TwitterResource>();
            PageTitle = Resource != null ? Resource.Title : "";

            this.PropertyChanged += (sender, property) =>
            {
                if (property.PropertyName == "Listbox" && Listbox != null)
                    UpdateTweetLoader();
            };

            AddCommand = new DelegateCommand((param) =>
            {
                if (!Config.Columns.Value.Contains(Resource))
                    Config.Columns.Value.Add(Resource);

                Config.SaveColumns();
                Notificator.ShowMessage(Localization.Resources.ColumnAdded);
                AddCommand.RaiseCanExecuteChanged();
            }, (param) => Resource != null && !Config.Columns.Value.Contains(Resource));
        }

        public void UpdateTweetLoader()
        {
            AddCommand.RaiseCanExecuteChanged();
            Listbox.Resource = Resource;
            Listbox.Load();
        }
    }
}
