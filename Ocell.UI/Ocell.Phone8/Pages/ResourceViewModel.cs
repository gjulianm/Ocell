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
        public static TwitterResource Resource;

        public string PageTitle { get; set; }

        public TweetLoader Loader { get; set; }

        public ExtendedListBox Listbox { get; set; }

        DelegateCommand addCommand;
        public ICommand AddCommand
        {
            get { return addCommand; }
        }

        public ResourceViewModel()
            : base("Search")
        {
            PageTitle = Resource != null ? Resource.Title : "";

            this.PropertyChanged += (sender, property) =>
                {
                    if (property.PropertyName == "Listbox")
                        UpdateTweetLoader();
                };

            addCommand = new DelegateCommand((param) =>
                {
                    if (!Config.Columns.Contains(Resource))
                        Config.Columns.Add(Resource);
                    Config.SaveColumns();
                    MessageService.ShowMessage(Localization.Resources.ColumnAdded, "");
                    DataTransfer.ShouldReloadColumns = true;
                    addCommand.RaiseCanExecuteChanged();
                },
                (param) => Resource != null && !Config.Columns.Contains(Resource));
        }

        public void UpdateTweetLoader()
        {
            addCommand.RaiseCanExecuteChanged();
            Listbox.Resource = Resource;
            Listbox.Load();
        }
    }
}
