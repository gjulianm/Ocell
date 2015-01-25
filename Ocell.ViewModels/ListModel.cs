using AncoraMVVM.Base;
using AncoraMVVM.Base.Collections;
using AncoraMVVM.Base.Diagnostics;
using Ocell.Library.Twitter;
using Ocell.Library.Twitter.Comparers;
using Ocell.Localization;
using PropertyChanged;
using System.Threading.Tasks;
using TweetSharp;

namespace Ocell.ViewModels
{
    [ImplementPropertyChanged]
    public class ListModel : ExtendedViewModelBase
    {
        public string ListName { get; set; }
        public SafeObservable<TwitterUser> ListUsers { get; private set; }
        public TweetLoader Loader { get; set; }
        public string FilterText { get; set; }
        public SortedFilteredObservable<TwitterUser> UserSearchResult { get; private set; }
        public bool CanFindMoreUsers { get; set; }
        public DelegateCommand AddUser { get; set; }
        public DelegateCommand RemoveUser { get; set; }

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
            };

            Loader = new TweetLoader(resource, false);
            CanFindMoreUsers = false;
            ListName = resource.Title;
            ListUsers = new SafeObservable<TwitterUser>();
            UserSearchResult = new SortedFilteredObservable<TwitterUser>(new TwitterUserComparer());


            AddUser = new DelegateCommand(AddUserToList);
            RemoveUser = new DelegateCommand(RemoveUserFromList);

            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "FilterText")
                    UpdateUserSearch();
            };
        }

        private async void AddUserToList(object param)
        {
            var user = (TwitterUser)param;

            Progress.IsLoading = true;
            var service = ServiceDispatcher.GetService(resource.User);

            var response = await service.RemoveListMemberAsync(new RemoveListMemberOptions
            {
                OwnerScreenName = resource.Data.Substring(1, resource.Data.IndexOf('/') - 1),
                Slug = resource.Data.Substring(resource.Data.IndexOf('/') + 1),
                UserId = user.Id
            });

            Progress.IsLoading = false;

            if (response.RequestSucceeded)
            {
                Notificator.ShowProgressIndicatorMessage(Resources.UserRemovedFromList);
            }
            else
            {
                var errorMessage = response.Error == null ? response.StatusCode.ToString() : response.Error.Message;
                Notificator.ShowError(string.Format(Resources.ErrorRemovingUser, errorMessage));
            }

            ListUsers.Add(user);
        }


        private async void RemoveUserFromList(object param)
        {
            var user = (TwitterUser)param;

            Progress.IsLoading = true;
            var service = ServiceDispatcher.GetService(resource.User);

            var response = await service.AddListMemberAsync(new AddListMemberOptions
            {
                OwnerScreenName = resource.Data.Substring(1, resource.Data.IndexOf('/') - 1),
                Slug = resource.Data.Substring(resource.Data.IndexOf('/') + 1),
                UserId = user.Id
            });

            Progress.IsLoading = false;

            if (response.RequestSucceeded)
            {
                Notificator.ShowProgressIndicatorMessage(Resources.UserAddedToList);
            }
            else
            {
                var errorMessage = response.Error == null ? response.StatusCode.ToString() : response.Error.Message;
                Notificator.ShowError(string.Format(Resources.ErrorAddingUserToList, errorMessage));
            }

            ListUsers.Remove(user);
        }

        private void UpdateUserSearch()
        {
            UserSearchResult.Discarder = (user) => !(user.ScreenName.Contains(FilterText) || user.Name.Contains(FilterText));
        }

        public override async void OnLoad()
        {
            await LoadListUsers();
        }

        private async Task LoadListUsers(long? nextCursor = null)
        {
            var service = ServiceDispatcher.GetService(resource.User);

            var response = await service.ListListMembersAsync(new ListListMembersOptions
            {
                SkipStatus = true,
                OwnerScreenName = resource.Data.Substring(1, resource.Data.IndexOf('/') - 1),
                Slug = resource.Data.Substring(resource.Data.IndexOf('/') + 1),
                Cursor = nextCursor
            });

            if (response.RequestSucceeded)
            {
                ListUsers.AddRange(response.Content);

                if (response.Content.NextCursor != 0)
                    await LoadListUsers(response.Content.NextCursor);
            }
        }
    }
}
