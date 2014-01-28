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
using PropertyChanged;

namespace Ocell.Pages.Search
{
    [ImplementPropertyChanged]
    public class EnterSearchModel : ExtendedViewModelBase
    {
        string query;

        public string Query
        {
            get
            {
                return query;
            }
            set
            {
                Assign("Query", ref query, value);
            }
        }

        readonly DelegateCommand buttonClick;

        public ICommand ButtonClick
        {
            get
            {
                return buttonClick;
            }
        }

        public EnterSearchModel()
            : base("EnterSearch")
        {
            this.PropertyChanged += (sender, e) =>
            {
                buttonClick.RaiseCanExecuteChanged();
            };

            buttonClick = new DelegateCommand((obj) =>
                {
                    var resource = new TwitterResource
                    {
                        User = DataTransfer.CurrentAccount ?? Config.Accounts.FirstOrDefault(),
                        Type = ResourceType.Search,
                        Data = Query
                    };

                    ResourceViewModel.Resource = resource;
                    Navigate(Uris.ResourceView);
                }, (obj) => !string.IsNullOrWhiteSpace(Query));
        }
    }
}
