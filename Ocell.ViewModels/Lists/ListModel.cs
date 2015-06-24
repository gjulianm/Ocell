using System;
using System.Linq;
using System.Threading.Tasks;
using AncoraMVVM.Base;
using AncoraMVVM.Base.Collections;
using AncoraMVVM.Base.Diagnostics;
using Ocell.Library;
using Ocell.Library.Extension;
using Ocell.Library.RuntimeData;
using Ocell.Library.Twitter;
using Ocell.Library.Twitter.Comparers;
using Ocell.Localization;
using PropertyChanged;
using TweetSharp;
using LogLevel = AncoraMVVM.Base.Diagnostics.LogLevel;

namespace Ocell.ViewModels.Lists
{
    [ImplementPropertyChanged]
    public sealed class ListModel : ExtendedViewModelBase
    {
        public string ListName { get; set; }
        public SafeObservable<TwitterUser> ListUsers { get; private set; }
        public TweetLoader Loader { get; set; }
        public string FilterText { get; set; }
        public SortedFilteredObservable<TwitterUser> UserSearchResult { get; private set; }
        public bool CanFindMoreUsers { get; set; }
        public DelegateCommand AddUser { get; set; }
        public DelegateCommand RemoveUser { get; set; }
        public DelegateCommand FindMoreUsers { get; set; }

        TwitterResource resource;

        public ListModel()
        {
            resource = ReceiveMessage<TwitterResource>();

            if (resource == null || resource.Type != ResourceType.List)
            {
                AncoraLogger.Instance.LogEvent("ListModel received no list", LogLevel.Error);
                Navigator.GoBack();
                Notificator.ShowError(Resources.ErrorUnexpected);
                return;
            }

            Loader = new TweetLoader(resource, false);

            Loader.LoadCacheAsync();
            Loader.Load();

            CanFindMoreUsers = true;
            ListName = resource.Title.ToUpperInvariant();
            ListUsers = new SafeObservable<TwitterUser>();
            UserSearchResult = new SortedFilteredObservable<TwitterUser>(new TwitterUserComparer());

            var userProvider = ApplicationData.UserProviders.GetForUser(resource.User);
            UserSearchResult.AddRange(userProvider.Users);

            var replayer = new ObservableCollectionReplayer();
            replayer.ReplayTo(userProvider.Users, UserSearchResult);

            AddUser = new DelegateCommand(AddUserToList);
            RemoveUser = new DelegateCommand(RemoveUserFromList);
            FindMoreUsers = new DelegateCommand(SearchUsers);

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "FilterText")
                    UpdateUserSearch();
            };
        }

        private int? searchPage = 1;
        private string previousSearchText = null;

        private async void SearchUsers()
        {
            var service = ServiceDispatcher.GetCurrentService();

            Progress.Loading();

            if (previousSearchText != FilterText)
                searchPage = 1;

            var response = await service.SearchForUserAsync(new SearchForUserOptions
            {
                Count = 100,
                Q = FilterText,
                Page = searchPage
            });

            Progress.Finished();

            if (!response.RequestSucceeded)
            {
                Notificator.ShowTwitterError(Resources.Error, response.Error);
                return;
            }

            searchPage++;

            var toAdd = response.Content.Where(x => UserSearchResult.All(y => x.ScreenName != y.ScreenName));

            UserSearchResult.AddRange(toAdd);
        }

        private async void AddUserToList(object param)
        {
            var user = (TwitterUser)param;

            Progress.IsLoading = true;
            var service = ServiceDispatcher.GetService(resource.User);

            var response = await service.AddListMemberAsync(new AddListMemberOptions
            {
                OwnerScreenName = resource.User.ScreenName,
                Slug = resource.Data,
                UserId = user.Id
            });

            Progress.IsLoading = false;

            if (response.RequestSucceeded)
            {
                Notificator.ShowProgressIndicatorMessage(Resources.UserAddedToList);
                ListUsers.Add(user);
                UserSearchResult.Remove(user);
            }
            else
            {
                var errorMessage = response.Error == null ? response.StatusCode.ToString() : response.Error.Message;
                Notificator.ShowError(string.Format(Resources.ErrorRemovingUser, errorMessage));
            }

        }


        private async void RemoveUserFromList(object param)
        {
            var user = (TwitterUser)param;

            Progress.IsLoading = true;
            var service = ServiceDispatcher.GetService(resource.User);

            var response = await service.AddListMemberAsync(new AddListMemberOptions
            {
                OwnerScreenName = resource.User.ScreenName,
                Slug = resource.Data,
                UserId = user.Id
            });

            Progress.IsLoading = false;

            if (response.RequestSucceeded)
            {
                Notificator.ShowProgressIndicatorMessage(Resources.UserAddedToList);
                ListUsers.Remove(user);
                UserSearchResult.Add(user);
            }
            else
            {
                var errorMessage = response.Error == null ? response.StatusCode.ToString() : response.Error.Message;
                Notificator.ShowError(string.Format(Resources.ErrorAddingUserToList, errorMessage));
            }

        }

        private void UpdateUserSearch()
        {
            Func<TwitterUser, bool> matches = user => user.ScreenName.Contains(FilterText) || user.Name.Contains(FilterText);
            UserSearchResult.Discarder =
                user => (!string.IsNullOrWhiteSpace(FilterText) && !matches(user)) || ListUsers.Contains(user);
        }

        public override async void OnLoad()
        {
            await LoadListUsers();
        }

        private async Task LoadListUsers(long? nextCursor = null)
        {
            Progress.Loading(Resources.LoadingLists);
            var service = ServiceDispatcher.GetService(resource.User);

            var response = await service.ListListMembersAsync(new ListListMembersOptions
            {
                SkipStatus = true,
                OwnerScreenName = resource.User.ScreenName,
                Slug = resource.Data,
                Cursor = nextCursor
            });

            Progress.Finished();

            if (response.RequestSucceeded)
            {
                ListUsers.AddRange(response.Content);

                if (response.Content.NextCursor != 0)
                    await LoadListUsers(response.Content.NextCursor);
            }
            else
            {
                Notificator.ShowError(Resources.ErrorLoadingListUsers);
            }
        }
    }
}
