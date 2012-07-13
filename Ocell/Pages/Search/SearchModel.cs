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

namespace Ocell.Pages.Search
{
    public class SearchModel : ExtendedViewModelBase
    {
        string query;
        public string Query
        {
            get { return query; }
            set { Assign("Query", ref query, value); }
        }

        TweetLoader loader;
        public TweetLoader Loader
        {
            get { return loader; }
            set { Assign("Loader", ref loader, value); }
        }

        DelegateCommand addCommand;
        public ICommand AddCommand
        {
            get { return addCommand; }
        }

        public SearchModel()
            : base("Search")
        {
            this.PropertyChanged += (sender, property) =>
                {
                    if (property.PropertyName == "Loader")
                        UpdateTweetLoader();
                };

            addCommand = new DelegateCommand((param) =>
                {
                    Config.Columns.Add(Loader.Resource);
                    Config.SaveColumns();
                    MessageService.ShowMessage("Search column added to main page.", "");
                    DataTransfer.ShouldReloadColumns = true;                    
                    addCommand.RaiseCanExecuteChanged();
                },
                (param) => !string.IsNullOrWhiteSpace(Query) && !Config.Columns.Any((column) => column.Type == ResourceType.Search && column.Data == Query));
        }

        public void UpdateTweetLoader()
        {
            Loader.Resource = new TwitterResource 
            { 
                User = DataTransfer.CurrentAccount == null ? DataTransfer.CurrentAccount : Config.Accounts[0],
                Data = Query, 
                Type = ResourceType.Search 
            };
            Loader.Load();
        }
    }
}
