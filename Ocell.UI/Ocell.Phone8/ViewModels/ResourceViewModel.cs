using AncoraMVVM.Base;
using Ocell.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using PropertyChanged;
using System.Windows.Input;

namespace Ocell.Pages.Search
{
    [ImplementPropertyChanged]
    public class ResourceViewModel : ExtendedViewModelBase
    {
        public string PageTitle { get; set; }
        public TweetLoader Loader { get; set; }
        public ExtendedListBox Listbox { get; set; }
        public TwitterResource Resource { get; set; }

        DelegateCommand addCommand;
        public ICommand AddCommand
        {
            get { return addCommand; }
        }

        public ResourceViewModel()
        {
            Resource = ReceiveMessage<TwitterResource>();
            PageTitle = Resource != null ? Resource.Title : "";

            this.PropertyChanged += (sender, property) =>
            {
                if (property.PropertyName == "Listbox" && Listbox != null)
                    UpdateTweetLoader();
            };

            addCommand = new DelegateCommand((param) =>
            {
                if (!Config.Columns.Value.Contains(Resource))
                    Config.Columns.Value.Add(Resource);

                Config.SaveColumns();
                Notificator.ShowMessage(Localization.Resources.ColumnAdded);
                addCommand.RaiseCanExecuteChanged();
            }, (param) => Resource != null && !Config.Columns.Value.Contains(Resource));
        }

        public void UpdateTweetLoader()
        {
            addCommand.RaiseCanExecuteChanged();
            Listbox.Resource = Resource;
            Listbox.Load();
        }
    }
}
