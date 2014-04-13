using AncoraMVVM.Base;
using AncoraMVVM.Base.Interfaces;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Pages.Search;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TweetSharp;

namespace Ocell.Pages.Columns
{
    [ImplementPropertyChanged]
    public class ColumnViewModel : ExtendedViewModelBase
    {
        public SafeObservable<ColumnViewPivotModel> Pivots { get; set; }

        public bool FastAddMode { get; set; }

        DelegateCommand enableQuickAdd;
        public ICommand EnableQuickAdd
        {
            get { return enableQuickAdd; }
        }

        DelegateCommand disableQuickAdd;
        public ICommand DisableQuickAdd
        {
            get { return disableQuickAdd; }
        }

        public ColumnViewModel()
        {
            Pivots = new SafeObservable<ColumnViewPivotModel>();

            enableQuickAdd = new DelegateCommand((param) => FastAddMode = true);
            disableQuickAdd = new DelegateCommand((param) => FastAddMode = false);

            this.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "FastAddMode")
                        foreach (var p in Pivots)
                            p.FastAddMode = FastAddMode;
                };

            foreach (var account in Config.Accounts.Value)
            {
                var pivot = new ColumnViewPivotModel(account);

                Pivots.Add(pivot);
            }
        }
    }

    public class ColumnViewPivotModel : ExtendedViewModelBase
    {
        static object listCacheSync = new object();
        static object searchesCacheSync = new object();
        static List<TwitterResource> listsCache = new List<TwitterResource>();
        static List<TwitterResource> searchesCache = new List<TwitterResource>();

        object resourcesSync = new object();
        public SafeObservable<TwitterResource> Resources { get; set; }

        UserToken user;
        object loadingSync = new object();

        public string Username { get; set; }
        public object SelectedResource { get; set; }
        public bool FastAddMode { get; set; }

        public ColumnViewPivotModel(UserToken User)
        {
            user = User;
            Username = user.ScreenName;

            Resources = new SafeObservable<TwitterResource>();

            this.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "SelectedResource" && SelectedResource is TwitterResource)
                    {
                        if (FastAddMode)
                            AddResource((TwitterResource)SelectedResource);
                        else
                            NavigateToResource((TwitterResource)SelectedResource);
                    }
                };

            GenerateCoreColumns();

            GetLists();
            GetSearches();
        }

        void GenerateCoreColumns()
        {
            Resources.Add(new TwitterResource
            {
                User = user,
                Type = ResourceType.Home
            });

            Resources.Add(new TwitterResource
            {
                User = user,
                Type = ResourceType.Mentions
            });

            Resources.Add(new TwitterResource
            {
                User = user,
                Type = ResourceType.Messages
            });

            Resources.Add(new TwitterResource
            {
                User = user,
                Type = ResourceType.Favorites
            });
        }

        void AddResource(TwitterResource resource)
        {
            if (Config.Columns.Value.Contains(resource))
                Notificator.ShowError(Localization.Resources.ColumnAlreadyPinned);
            else if (Notificator.Prompt(String.Format(Localization.Resources.AskAddColumn, resource.Title)))
            {
                Config.Columns.Value.Add(resource);
                Config.SaveColumns();
                DataTransfer.ShouldReloadColumns = true;
            }
        }

        void NavigateToResource(TwitterResource resource)
        {
            Navigator.MessageAndNavigate<ResourceViewModel, TwitterResource>(resource);
        }

        void GetLists()
        {
            List<TwitterResource> userLists;

            lock (listCacheSync)
                userLists = listsCache.Where(x => x.User == user).ToList();

            if (userLists.Count > 0)
            {
                lock (resourcesSync)
                    foreach (var item in userLists.Except(Resources))
                        Resources.Add(item);
            }
            else
            {
                var service = ServiceDispatcher.GetService(user);

                Progress.IsLoading = true;
                service.ListListsForAsync(new ListListsForOptions { ScreenName = user.ScreenName }).ContinueWith(ReceiveLists);

                Progress.IsLoading = true;
                service.ListSubscriptionsAsync(new ListSubscriptionsOptions { ScreenName = user.ScreenName }).ContinueWith(ReceiveSubscriptions);
            }
        }

        private void ReceiveSubscriptions(Task<TwitterResponse<TwitterCursorList<TwitterList>>> task)
        {
            var response = task.Result;

            Progress.IsLoading = false;

            if (!response.RequestSucceeded)
                Notificator.ShowError(Localization.Resources.ErrorLoadingLists);
            else
                AddLists(response.Content);
        }

        private void ReceiveLists(Task<TwitterResponse<IEnumerable<TwitterList>>> task)
        {
            var response = task.Result;

            Progress.IsLoading = false;

            if (!response.RequestSucceeded)
                Notificator.ShowError(Localization.Resources.ErrorLoadingLists);
            else
                AddLists(response.Content);
        }

        void AddLists(IEnumerable<TwitterList> lists)
        {
            lock (resourcesSync)
            {
                foreach (var item in lists)
                {
                    if (!Resources.Any(x => x.Data == item.FullName && x.Type == ResourceType.List))
                        Resources.Add(new TwitterResource
                        {
                            User = user,
                            Type = ResourceType.List,
                            Data = item.FullName
                        });
                }
            }

            lock (listCacheSync)
            {
                foreach (var item in Resources.Except(listsCache))
                    listsCache.Add(item);
            }


        }

        void GetSearches()
        {
            List<TwitterResource> userSearches;

            lock (listCacheSync)
                userSearches = searchesCache.Where(x => x.User == user).ToList();

            if (userSearches.Count > 0)
            {
                lock (resourcesSync)
                    foreach (var item in userSearches.Except(Resources))
                        Resources.Add(item);
            }
            else
            {
                GetSavedSearches();
            }
        }

        async void GetSavedSearches()
        {
            var service = ServiceDispatcher.GetService(user);

            Progress.IsLoading = true;
            var response = await service.ListSavedSearchesAsync();
            Progress.IsLoading = false;

            if (!response.RequestSucceeded)
            {
                Notificator.ShowError(Localization.Resources.ErrorDownloadingSearches);
                return;
            }

            var searches = response.Content;

            lock (resourcesSync)
            {
                foreach (var item in searches)
                {
                    if (!Resources.Any(x => x.Data == item.Query && x.Type == ResourceType.Search))
                        Resources.Add(new TwitterResource
                        {
                            User = user,
                            Type = ResourceType.Search,
                            Data = item.Query
                        });
                }
            }

            lock (searchesCacheSync)
            {
                foreach (var item in Resources.Except(searchesCache))
                    searchesCache.Add(item);
            }
        }
    }
}
