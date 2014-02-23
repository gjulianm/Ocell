using AncoraMVVM.Base;
using Ocell.Library;
using Ocell.Library.Twitter;
using PropertyChanged;
using System.Linq;
using System.Windows.Input;

namespace Ocell.Pages.Search
{
    [ImplementPropertyChanged]
    public class EnterSearchModel : ExtendedViewModelBase
    {
        public string Query { get; set; }

        readonly DelegateCommand buttonClick;

        public ICommand ButtonClick
        {
            get
            {
                return buttonClick;
            }
        }

        public EnterSearchModel()
        {
            this.PropertyChanged += (sender, e) =>
            {
                buttonClick.RaiseCanExecuteChanged();
            };

            buttonClick = new DelegateCommand((obj) =>
                {
                    var resource = new TwitterResource
                    {
                        User = DataTransfer.CurrentAccount ?? Config.Accounts.Value.FirstOrDefault(),
                        Type = ResourceType.Search,
                        Data = Query
                    };

                    ResourceViewModel.Resource = resource;
                    Navigator.Navigate(Uris.ResourceView);
                }, (obj) => !string.IsNullOrWhiteSpace(Query));
        }
    }
}
