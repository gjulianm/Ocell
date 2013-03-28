using DanielVaughan.Windows;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Pages.Search;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using TweetSharp;

namespace Ocell.Pages.Columns
{
    public class ColumnViewModel : ExtendedViewModelBase
    {
        SafeObservable<ColumnViewPivotModel> pivots;
        public SafeObservable<ColumnViewPivotModel> Pivots
        {
            get { return pivots; }
            set { Assign("Pivots", ref pivots, value); }
        }

        bool fastAddMode;
        public bool FastAddMode
        {
            get { return fastAddMode; }
            set { Assign("FastAddMode", ref fastAddMode, value); }
        }

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

        int pivotsLoading = 0;

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

            foreach (var account in Config.Accounts)
            {
                var pivot = new ColumnViewPivotModel(account);

                pivot.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == "IsLoading")
                        {
                            var p = sender as ColumnViewPivotModel;

                            if (p == null)
                                return;

                            if (p.IsLoading)
                                pivotsLoading++;
                            else
                                pivotsLoading--;

                            IsLoading = pivotsLoading > 0;
                        }
                    };

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
        SafeObservable<TwitterResource> resources;
        public SafeObservable<TwitterResource> Resources
        {
            get { return resources; }
            set { Assign("Resources", ref resources, value); }
        }


        UserToken user;
        int loading = 0;
        object loadingSync = new object();

        string username;
        public string Username
        {
            get { return username; }
            set { Assign("Username", ref username, value); }
        }

        object selectedResource;
        public object SelectedResource
        {
            get { return selectedResource; }
            set { Assign("SelectedResource", ref selectedResource, value); }
        }

        public bool FastAddMode { get; set; }

        public ColumnViewPivotModel(UserToken User)
        {
            user = User;
            Username = user.ScreenName;

            Resources = new SafeObservable<TwitterResource>();

            this.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "SelectedResource" && selectedResource is TwitterResource)
                    {
                        if (FastAddMode)
                            AddResource((TwitterResource)selectedResource);
                        else
                            NavigateToResource((TwitterResource)selectedResource);
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
            if (Config.Columns.Contains(resource))
                MessageService.ShowError(Localization.Resources.ColumnAlreadyPinned);
            else if (MessageService.AskYesNoQuestion(String.Format(Localization.Resources.AskAddColumn, resource.Title)))
            {
                Config.Columns.Add(resource);
                Config.SaveColumns();
                DataTransfer.ShouldReloadColumns = true;
            }
        }

        void NavigateToResource(TwitterResource resource)
        {
            ResourceViewModel.Resource = resource;
            Navigate(Uris.ResourceView);
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

                loading += 2;
                service.ListListsFor(new ListListsForOptions { ScreenName = user.ScreenName }, ReceiveLists);
                service.ListSubscriptions(new ListSubscriptionsOptions { ScreenName = user.ScreenName }, ReceiveLists);
            }
        }

        void ReceiveLists(IEnumerable<TwitterList> list, TwitterResponse response)
        {
            loading--;

            if (loading <= 0)
                IsLoading = false;

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageService.ShowError(Localization.Resources.ErrorLoadingLists);
                return;
            }

            lock (resourcesSync)
            {
                foreach (var item in list)
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
                var service = ServiceDispatcher.GetService(user);

                loading++;
                service.ListSavedSearches(ReceiveSearches);
            }
        }

        void ReceiveSearches(IEnumerable<TwitterSavedSearch> searches, TwitterResponse response)
        {
            loading--;

            if (loading <= 0)
                IsLoading = false;

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageService.ShowError(Localization.Resources.ErrorDownloadingSearches);
                return;
            }

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
