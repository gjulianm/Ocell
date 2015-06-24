using AncoraMVVM.Base;
using Ocell.Library;
using Ocell.Library.Twitter;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ocell.Library.RuntimeData;
using TweetSharp;

namespace Ocell.Pages.Columns
{
    [ImplementPropertyChanged]
    public class AddColumnModel : ExtendedViewModelBase
    {
        public SafeObservable<TwitterList> Lists { get; set; }

        public ObservableCollection<TwitterResource> Core { get; set; }

        public object CoreSelection { get; set; }

        public object ListSelection { get; set; }

        DelegateCommand reloadLists;
        public ICommand ReloadLists
        {
            get { return reloadLists; }
        }

        public AddColumnModel()
        {
            Core = new ObservableCollection<TwitterResource>();
            Lists = new SafeObservable<TwitterList>();

            reloadLists = new DelegateCommand(() => LoadLists(), () => !Progress.IsLoading);

            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "ListSelection")
                    AddSelectedList();
                else if (e.PropertyName == "CoreSelection")
                    AddSelectedCoreResource();
            };
        }

        int loading = 0;
        object sync = new object();

        public override void OnLoad()
        {
            if (ApplicationData.CurrentAccount == null)
            {
                Notificator.ShowError(Localization.Resources.ErrorNoAccount);
                Navigator.GoBack();
                return;
            }

            CreateCoreList();

            LoadLists();
        }

        private void LoadLists()
        {
            var service = ServiceDispatcher.GetService(ApplicationData.CurrentAccount);

            Progress.IsLoading = true;
            Progress.Text = Localization.Resources.LoadingLists;

            loading = 2;
            service.ListListsForAsync(new ListListsForOptions { ScreenName = ApplicationData.CurrentAccount.ScreenName }).ContinueWith(ReceiveLists);
            service.ListSubscriptionsAsync(new ListSubscriptionsOptions { ScreenName = ApplicationData.CurrentAccount.ScreenName }).ContinueWith(ReceiveSubscriptions);
        }



        private void CreateCoreList()
        {
            Core.Add(new TwitterResource
            {
                User = ApplicationData.CurrentAccount,
                Type = ResourceType.Home
            });

            Core.Add(new TwitterResource
            {
                User = ApplicationData.CurrentAccount,
                Type = ResourceType.Mentions
            });

            Core.Add(new TwitterResource
            {
                User = ApplicationData.CurrentAccount,
                Type = ResourceType.Messages
            });

            Core.Add(new TwitterResource
            {
                User = ApplicationData.CurrentAccount,
                Type = ResourceType.Favorites
            });
        }

        private void ReceiveSubscriptions(Task<TwitterResponse<TwitterCursorList<TwitterList>>> task)
        {
            var response = task.Result;
            if (!response.RequestSucceeded)
                Notificator.ShowError(Localization.Resources.ErrorLoadingLists);

            AddLists(response.Content);
        }

        private void ReceiveLists(Task<TwitterResponse<IEnumerable<TwitterList>>> task)
        {
            var response = task.Result;
            if (!response.RequestSucceeded)
                Notificator.ShowError(Localization.Resources.ErrorLoadingLists);

            AddLists(response.Content);
        }

        private void AddLists(IEnumerable<TwitterList> lists)
        {
            lock (sync)
            {
                loading--;

                if (loading <= 0)
                {
                    Progress.IsLoading = false;
                    Progress.Text = "";
                    reloadLists.RaiseCanExecuteChanged();
                }
            }

            foreach (var list in lists)
                if (!Lists.Any(x => x.FullName == list.FullName))
                    Lists.Add(list);
        }

        void AddSelectedList()
        {
            var list = ListSelection as TwitterList;
            if (list == null)
                return;

            var toAdd = new TwitterResource
            {
                Type = ResourceType.List,
                Data = list.FullName,
                User = ApplicationData.CurrentAccount
            };

            SaveColumn(toAdd);
            ListSelection = null;
            Navigator.GoBack();
        }

        void AddSelectedCoreResource()
        {
            if (CoreSelection == null || !(CoreSelection is TwitterResource))
                return;

            SaveColumn((TwitterResource)CoreSelection);
            CoreSelection = null;
            Navigator.GoBack();
        }

        private void SaveColumn(TwitterResource toAdd)
        {
            if (!Config.Columns.Value.Contains(toAdd))
            {
                Config.Columns.Value.Add(toAdd);
                Config.SaveColumns();
            }
        }
    }
}
