using AncoraMVVM.Base;
using Ocell.Library.Twitter;
using Ocell.Localization;
using PropertyChanged;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Data;
using TweetSharp;

namespace Ocell.Pages.Elements
{
    [ImplementPropertyChanged]
    public class UserListModel : ExtendedViewModelBase
    {
        string whatUserList;
        string user;

        public string PageTitle { get; set; }

        SafeObservable<TwitterUser> list;

        CollectionViewSource viewSource;

        public System.ComponentModel.ICollectionView List
        {
            get { return viewSource.View; }
        }

        public object SelectedUser { get; set; }

        public UserListModel()
            : base("UserList")
        {
            whatUserList = "";
            user = "";
            PageTitle = whatUserList;
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
                        Navigate("/Pages/Elements/User.xaml?user=" + selected.ScreenName);
                        SelectedUser = null;
                    }
                }
            };
        }

        public void Loaded(string resource, string userName)
        {
            whatUserList = resource;
            user = userName;

            if (whatUserList == "followers")
            {
                ServiceDispatcher.GetCurrentService().ListFollowersAsync(new ListFollowersOptions { ScreenName = user, IncludeUserEntities = true }).ContinueWith(ReceiveUsers);
                BarText = Resources.DownloadingFollowers;
                PageTitle = Resources.Followers;
            }
            else if (whatUserList == "following")
            {
                ServiceDispatcher.GetCurrentService().ListFriendsAsync(new ListFriendsOptions { ScreenName = user, IncludeUserEntities = true }).ContinueWith(ReceiveUsers);
                BarText = Resources.DownloadingFollowing;
                PageTitle = Resources.Following;
            }
            else
            {
                MessageService.ShowError(Resources.NotValidResource);
                GoBack();
                return;
            }

            Progress.IsLoading = true;
        }


        private void ReceiveUsers(Task<TwitterResponse<TwitterCursorList<TwitterUser>>> task)
        {
            var response = task.Result;

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                MessageService.ShowError(Resources.CouldntFindUser);
                GoBack();
                return;
            }
            else if (!response.RequestSucceeded)
            {
                MessageService.ShowError(Resources.ErrorMessage);
                GoBack();
                return;
            }

            var users = response.Content;

            foreach (var usr in users)
                if (!list.Contains(usr))
                    list.Add(usr);

            if (users.NextCursor != null && users.NextCursor != 0)
            {
                if (whatUserList == "followers")
                    ServiceDispatcher.GetCurrentService().ListFollowersAsync(new ListFollowersOptions { ScreenName = user, Cursor = users.NextCursor }).ContinueWith(ReceiveUsers);
                else if (whatUserList == "following")
                    ServiceDispatcher.GetCurrentService().ListFriendsAsync(new ListFriendsOptions { ScreenName = user, Cursor = users.NextCursor }).ContinueWith(ReceiveUsers);
            }
            else
            {
                Progress.IsLoading = false;
            }
        }
    }
}
