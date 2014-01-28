using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DanielVaughan.ComponentModel;
using DanielVaughan;
using DanielVaughan.Windows;
using Ocell.Library.Twitter;
using Ocell.Library;
using System.Linq;
using Ocell.Controls;
using PropertyChanged;

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
