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
using System.Collections.ObjectModel;
using TweetSharp;
using Ocell.Library.Twitter;
using Ocell.Library;
using System.Collections.Generic;
using System.Threading;
using DanielVaughan.Windows;
using System.Linq;
using System.Threading.Tasks;

namespace Ocell.Pages.Columns
{
    public class AddColumnModel : ExtendedViewModelBase
    {
        SafeObservable<TwitterList> lists;
        public SafeObservable<TwitterList> Lists
        {
            get { return lists; }
            set { Assign("Lists", ref lists, value); }
        }

        ObservableCollection<TwitterResource> core;
        public ObservableCollection<TwitterResource> Core
        {
            get { return core; }
            set { Assign("Core", ref core, value); }
        }

        object coreSelection;
        public object CoreSelection
        {
            get { return coreSelection; }
            set { Assign("CoreSelection", ref coreSelection, value); }
        }

        object listSelection;
        public object ListSelection
        {
            get { return listSelection; }
            set { Assign("ListSelection", ref listSelection, value); }
        }

        DelegateCommand reloadLists;
        public ICommand ReloadLists
        {
            get { return reloadLists; }
        }

        public AddColumnModel()
            : base("AddColumn")
        {
            Core = new ObservableCollection<TwitterResource>();
            Lists = new SafeObservable<TwitterList>();

            reloadLists = new DelegateCommand((param) => LoadLists(), (param) => !IsLoading);

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

        public void OnLoad()
        {
            if (DataTransfer.CurrentAccount == null)
            {
                MessageService.ShowError(Localization.Resources.ErrorNoAccount);
                GoBack();
                return;
            }

            CreateCoreList();

            LoadLists();
        }

        private void LoadLists()
        {
            var service = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);

            IsLoading = true;
            BarText = Localization.Resources.LoadingLists;

            loading = 2;
            service.ListListsForAsync(new ListListsForOptions { ScreenName = DataTransfer.CurrentAccount.ScreenName }).ContinueWith(ReceiveLists);
            service.ListSubscriptionsAsync(new ListSubscriptionsOptions { ScreenName = DataTransfer.CurrentAccount.ScreenName }).ContinueWith(ReceiveSubscriptions);
        }

       

        private void CreateCoreList()
        {
            Core.Add(new TwitterResource
            {
                User = DataTransfer.CurrentAccount,
                Type = ResourceType.Home
            });

            Core.Add(new TwitterResource
            {
                User = DataTransfer.CurrentAccount,
                Type = ResourceType.Mentions
            });

            Core.Add(new TwitterResource
            {
                User = DataTransfer.CurrentAccount,
                Type = ResourceType.Messages
            });

            Core.Add(new TwitterResource
            {
                User = DataTransfer.CurrentAccount,
                Type = ResourceType.Favorites
            });
        }

        private void ReceiveSubscriptions(Task<TwitterResponse<TwitterCursorList<TwitterList>>> task)
        {
            var response = task.Result;
            if (!response.RequestSucceeded)
                MessageService.ShowError(Localization.Resources.ErrorLoadingLists);

            AddLists(response.Content);
        }

        private void ReceiveLists(Task<TwitterResponse<IEnumerable<TwitterList>>> task)
        {
            var response = task.Result;
            if (!response.RequestSucceeded)
                MessageService.ShowError(Localization.Resources.ErrorLoadingLists);

            AddLists(response.Content);
        }

        private void AddLists(IEnumerable<TwitterList> lists)
        {
            lock (sync)
            {
                loading--;

                if (loading <= 0)
                {
                    IsLoading = false;
                    BarText = "";
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
                User = DataTransfer.CurrentAccount
            };

            SaveColumn(toAdd);           
            ListSelection = null;
            GoBack();        
        }

        void AddSelectedCoreResource()
        {
            if (CoreSelection == null || !(CoreSelection is TwitterResource))
                return;

            SaveColumn((TwitterResource)CoreSelection);
            CoreSelection = null;
            GoBack();
        }

        private void SaveColumn(TwitterResource toAdd)
        {
            if (!Config.Columns.Contains(toAdd))
            {
                Config.Columns.Add(toAdd);
                Config.SaveColumns();
                DataTransfer.ShouldReloadColumns = true;
            }
        }
    }
}
