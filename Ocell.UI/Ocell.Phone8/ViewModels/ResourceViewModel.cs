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
        {
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
                    DataTransfer.ShouldReloadColumns = true;
                    addCommand.RaiseCanExecuteChanged();
                },
                (param) => Resource != null && !Config.Columns.Value.Contains(Resource));
        }

        public void UpdateTweetLoader()
        {
            addCommand.RaiseCanExecuteChanged();
            Listbox.Resource = Resource;
            Listbox.Load();
        }
    }
}
