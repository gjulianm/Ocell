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

namespace Ocell.Pages.Search
{
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
                    Navigate(new Uri("/Pages/Search/Search.xaml?form=1&q=" + Uri.EscapeDataString(Query), UriKind.Relative));
                }, (obj) => !string.IsNullOrWhiteSpace(Query));
        }
    }
}
