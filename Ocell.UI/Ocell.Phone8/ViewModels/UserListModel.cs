using AncoraMVVM.Base;
using AncoraMVVM.Base.Diagnostics;
using Ocell.Library.Twitter;
using Ocell.Localization;
using PropertyChanged;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Data;
using TweetSharp;

namespace Ocell.Pages.Elements
{
    [Flags]
    public enum UserListResource { Following, Followers }

    public class UserListParams
    {
        public UserListResource Resource { get; set; }
        public string User { get; set; }
    }

    [ImplementPropertyChanged]
    public class UserListModel : ExtendedViewModelBase
    {
        SafeObservable<TwitterUser> list;
        CollectionViewSource viewSource;
        UserListParams toShow;

        public string PageTitle { get; set; }

        public System.ComponentModel.ICollectionView List
        {
            get { return viewSource.View; }
        }

        public object SelectedUser { get; set; }

        public UserListModel()
        {
            list = new SafeObservable<TwitterUser>();
            viewSource = new CollectionViewSource();
            viewSource.Source = list;
            viewSource.View.SortDescriptions.Add(new System.ComponentModel.SortDescription("ScreenName", System.ComponentModel.ListSortDirection.Ascending));

            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "SelectedUser")
                {
                    TwitterUser selected = SelectedUser as TwitterUser;
                    if (selected != null)
                    {
                        Navigator.Navigate("/Pages/Elements/User.xaml?user=" + selected.ScreenName);
                        SelectedUser = null;
                    }
                }
            };
        }

        public override void OnLoad()
        {
            toShow = ReceiveMessage<UserListParams>();

            if (toShow == null)
            {
                AncoraLogger.Instance.LogEvent("Loaded UserList but didn't receive parameters of what to show", LogLevel.Error);
                Notificator.ShowError(Resources.ErrorUnexpected);
                Navigator.GoBack();
                return;
            }

            if (toShow.Resource == UserListResource.Followers)
            {
                ServiceDispatcher.GetCurrentService().ListFollowersAsync(new ListFollowersOptions { ScreenName = toShow.User, IncludeUserEntities = true }).ContinueWith(ReceiveUsers);
                Progress.Text = Resources.DownloadingFollowers;
                PageTitle = Resources.Followers;
            }
            else if (toShow.Resource == UserListResource.Following)
            {
                ServiceDispatcher.GetCurrentService().ListFriendsAsync(new ListFriendsOptions { ScreenName = toShow.User, IncludeUserEntities = true }).ContinueWith(ReceiveUsers);
                Progress.Text = Resources.DownloadingFollowing;
                PageTitle = Resources.Following;
            }
            else
            {
                Notificator.ShowError(Resources.NotValidResource);
                Navigator.GoBack();
                return;
            }

            Progress.IsLoading = true;
        }


        private void ReceiveUsers(Task<TwitterResponse<TwitterCursorList<TwitterUser>>> task)
        {
            var response = task.Result;

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Notificator.ShowError(Resources.CouldntFindUser);
                Navigator.GoBack();
                return;
            }
            else if (!response.RequestSucceeded)
            {
                Notificator.ShowError(Resources.ErrorMessage);
                Navigator.GoBack();
                return;
            }

            var users = response.Content;

            foreach (var usr in users)
                if (!list.Contains(usr))
                    list.Add(usr);

            if (users.NextCursor != null && users.NextCursor != 0)
            {
                if (toShow.Resource == UserListResource.Followers)
                    ServiceDispatcher.GetCurrentService().ListFollowersAsync(new ListFollowersOptions { ScreenName = toShow.User, Cursor = users.NextCursor }).ContinueWith(ReceiveUsers);
                else if (toShow.Resource == UserListResource.Following)
                    ServiceDispatcher.GetCurrentService().ListFriendsAsync(new ListFriendsOptions { ScreenName = toShow.User, Cursor = users.NextCursor }).ContinueWith(ReceiveUsers);
            }
            else
            {
                Progress.IsLoading = false;
            }
        }
    }
}
