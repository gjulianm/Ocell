using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Input;
using DanielVaughan.ComponentModel;
using DanielVaughan.Windows;
using Microsoft.Phone.Tasks;
using Ocell.Library;
using Ocell.Library.Twitter;
using TweetSharp;
using Ocell.Localization;

namespace Ocell.Pages.Elements
{
    public class UserModel : ExtendedViewModelBase
    {
        public TwitterUser User { get; set; } // Not a property, don't need Assign().

        bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { Assign("IsLoading", ref isLoading, value); }
        }

        string barText;
        public string BarText
        {
            get { return barText; }
            set { Assign("BarText", ref barText, value); }
        }

        bool followed;
        public bool Followed
        {
            get { return followed; }
            set { Assign("Followed", ref followed, value); }
        }

        bool blocked;
        public bool Blocked
        {
            get { return blocked; }
            set { Assign("Blocked", ref blocked, value); }
        }

        bool isOwner;
        public bool IsOwner
        {
            get { return isOwner; }
            set { Assign("IsOwner", ref isOwner, value); }
        }

        #region Fields.
        string avatar;
        public string Avatar
        {
            get { return avatar; }
            set { Assign("Avatar", ref avatar, value); }
        }

        string name;
        public string Name
        {
            get { return name; }
            set { Assign("Name", ref name, value); }
        }

        string screenName;
        public string ScreenName
        {
            get { return screenName; }
            set { Assign("ScreenName", ref screenName, value); }
        }

        string website;
        public string Website
        {
            get { return website; }
            set { Assign("Website", ref website, value); }
        }

        string biography;
        public string Biography
        {
            get { return biography; }
            set { Assign("Biography", ref biography, value); }
        }

        string tweets;
        public string Tweets
        {
            get { return tweets; }
            set { Assign("Tweets", ref tweets, value); }
        }

        string following;
        public string Following
        {
            get { return following; }
            set { Assign("Following", ref following, value); }
        }

        string followers;
        public string Followers
        {
            get { return followers; }
            set { Assign("Followers", ref followers, value); }
        }

        bool websiteEnabled;
        public bool WebsiteEnabled
        {
            get { return websiteEnabled; }
            set { Assign("WebsiteEnabled", ref websiteEnabled, value); }
        }
        #endregion

        #region Commands
        Func<object, bool> GenericCanExecute;

        DelegateCommand followUser;
        public ICommand FollowUser
        {
            get { return followUser; }
        }

        DelegateCommand unfollowUser;
        public ICommand UnfollowUser
        {
            get { return unfollowUser; }
        }

        DelegateCommand pinUser;
        public ICommand PinUser
        {
            get { return pinUser; }
        }

        DelegateCommand block;
        public ICommand Block
        {
            get { return block; }
        }

        DelegateCommand unblock;
        public ICommand Unblock
        {
            get { return unblock; }
        }

        DelegateCommand reportSpam;
        public ICommand ReportSpam
        {
            get { return reportSpam; }
        }

        DelegateCommand manageLists;
        public ICommand ManageLists
        {
            get { return manageLists; }
        }

        DelegateCommand changeAvatar;
        public ICommand ChangeAvatar
        {
            get { return changeAvatar; }
        }

        DelegateCommand navigateTo;
        public ICommand NavigateTo
        {
            get { return navigateTo; }
        }

        #endregion

        public UserModel()
            : base("User")
        {
            GenericCanExecute = (obj) => User != null && DataTransfer.CurrentAccount != null;

            followUser = new DelegateCommand((obj) =>
            {
                IsLoading = true;
                ServiceDispatcher.GetService(DataTransfer.CurrentAccount).FollowUser(User.Id, ReceiveFollow);
            }, GenericCanExecute);

            unfollowUser = new DelegateCommand((obj) =>
            {
                IsLoading = true;
                ServiceDispatcher.GetService(DataTransfer.CurrentAccount).UnfollowUser(User.Id, ReceiveFollow);
            }, GenericCanExecute);

            pinUser = new DelegateCommand((obj) =>
                {
                    Config.Columns.Add(new TwitterResource
                    {
                        Data = User.ScreenName,
                        Type = ResourceType.Tweets,
                        User = DataTransfer.CurrentAccount
                    });
                    Config.SaveColumns();
                    MessageService.ShowLightNotification(Resources.UserPinned);
                    pinUser.RaiseCanExecuteChanged();

                }, item => GenericCanExecute.Invoke(null) 
                    && !Config.Columns.Any(o => o.Type == ResourceType.Tweets && o.Data == User.ScreenName));

            block = new DelegateCommand((obj) =>
                {
                    IsLoading = true;
                    ServiceDispatcher.GetService(DataTransfer.CurrentAccount).BlockUser(User.Id, ReceiveBlock);
                }, GenericCanExecute);

            unblock = new DelegateCommand((obj) =>
                {
                    IsLoading = true;
                    ServiceDispatcher.GetService(DataTransfer.CurrentAccount).UnblockUser(User.Id, ReceiveBlock);
                }, GenericCanExecute);

            reportSpam = new DelegateCommand((obj) =>
            {
                IsLoading = true;
                ServiceDispatcher.GetService(DataTransfer.CurrentAccount).ReportSpam(User.Id, ReceiveReportSpam);
            }, GenericCanExecute);

            changeAvatar = new DelegateCommand((obj) =>
                {
                    PhotoChooserTask task = new PhotoChooserTask();
                    task.ShowCamera = true;
                    task.Completed += new EventHandler<PhotoResult>(task_Completed);
                    task.Show();
                }, (obj) => GenericCanExecute.Invoke(null) && IsOwner);

            navigateTo = new DelegateCommand((url) =>
                {
                    var task = new WebBrowserTask();
                    task.Uri = new Uri((string)url, UriKind.Absolute);
                    task.Show();
                }, (url) => url is string && Uri.IsWellFormedUriString(url as string, UriKind.Absolute));

            manageLists = new DelegateCommand((obj) => Navigate("/Pages/Lists/ListManager.xaml?user=" + User.ScreenName),
                GenericCanExecute);
        }

        public void Loaded(string userName)
        {
            Regex remove = new Regex("@|:");
            userName = remove.Replace(userName, "");

            ScreenName = userName;

            BarText = Resources.RetrievingUser;
            IsLoading = true;
            ServiceDispatcher.GetDefaultService().ListUserProfilesFor(new List<string> { userName }, ReceiveUsers);
        }

        void task_Completed(object sender, PhotoResult e)
        {
            UserToken usr;
            usr = Config.Accounts.FirstOrDefault(item => item != null && item.ScreenName == User.ScreenName);
            if (e.TaskResult == TaskResult.OK && User != null)
            {
                BarText = Resources.UploadingPicture;
                IsLoading = true;
                ITwitterService srv = ServiceDispatcher.GetService(usr);
                srv.UpdateProfileImage(e.OriginalFileName, e.ChosenPhoto, ReceivePhotoUpload);
            }
        }

        private void ReceivePhotoUpload(TwitterUser user, TwitterResponse response)
        {
            BarText = "";
            IsLoading = false;
            if (response.StatusCode == HttpStatusCode.OK)
                MessageService.ShowLightNotification(Resources.ProfileImageChanged);
            else
                MessageService.ShowError(Resources.ErrorUploadingProfileImage);
        }

        void ReceiveFollow(TwitterUser usr, TwitterResponse response)
        {
            string successMsg = "", errorMsg = "";

            if (usr == null)
            {
                MessageService.ShowError(Resources.ErrorMessage);
                return;
            }

            if (!Followed)
            {
                successMsg = String.Format(Resources.NowYouFollow, usr.ScreenName);
                errorMsg = String.Format(Resources.CouldntFollow, usr.ScreenName);
            }
            else
            {
                successMsg = String.Format(Resources.NowYouUnfollow, usr.ScreenName);
                errorMsg = String.Format(Resources.CouldntUnfollow, usr.ScreenName);
            }

            IsLoading = false;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                MessageService.ShowLightNotification(successMsg);
                Followed = !Followed;
                followUser.RaiseCanExecuteChanged();
                unfollowUser.RaiseCanExecuteChanged(); ;
            }
            else
                MessageService.ShowError(errorMsg);
        }

        void ReceiveBlock(TwitterUser usr, TwitterResponse response)
        {
            string successMsg = "", errorMsg = "";

            if (!Blocked)
            {
                successMsg = String.Format(Resources.UserIsNowUnblocked, usr.ScreenName);
                errorMsg = String.Format(Resources.CouldntUnblock, usr.ScreenName);
            }
            else
            {
                successMsg = String.Format(Resources.UserIsNowBlocked, usr.ScreenName);
                errorMsg = String.Format(Resources.CouldntBlock, usr.ScreenName);
            }

            IsLoading = false;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                MessageService.ShowLightNotification(successMsg);
                Blocked = !Blocked;
                block.RaiseCanExecuteChanged();
                unblock.RaiseCanExecuteChanged();
            }
            else
                MessageService.ShowError(errorMsg);
        }

        void ReceiveReportSpam(TwitterUser usr, TwitterResponse response)
        {
            IsLoading = false;
            if (response.StatusCode == HttpStatusCode.OK)
                MessageService.ShowLightNotification(String.Format(Resources.ReportedAndBlocked, usr.ScreenName));
            else
                MessageService.ShowError(String.Format(Resources.CouldntReport, usr.ScreenName));
        }

        void ReceiveUsers(IEnumerable<TwitterUser> users, TwitterResponse response)
        {
            BarText = "";
            IsLoading = false;
            if (response.StatusCode != HttpStatusCode.OK || !users.Any())
            {
                MessageService.ShowError(Resources.CouldntFindUser);
                return;
            }

            User = users.First();

            Avatar = User.ProfileImageUrl;
            Name = User.Name;
            ScreenName = User.ScreenName;
            Website = User.Url;
            Biography = User.Description;
            Tweets = User.StatusesCount.ToString();
            Followers = User.FollowersCount.ToString();
            Following = User.FriendsCount.ToString();
            WebsiteEnabled = Uri.IsWellFormedUriString(User.Url, UriKind.Absolute);
            IsOwner = Config.Accounts.Any(item => item.Id == User.Id);

            if (DataTransfer.CurrentAccount != null)
            {
                var service = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);
                service.GetFriendshipInfo((int)DataTransfer.CurrentAccount.Id, User.Id, ReceiveFriendshipInfo);
                service.ListBlockedUserIds(ReceiveBlockedUsers);
            }

            followUser.RaiseCanExecuteChanged();
            unfollowUser.RaiseCanExecuteChanged();
            block.RaiseCanExecuteChanged();
            unblock.RaiseCanExecuteChanged();
            pinUser.RaiseCanExecuteChanged();
            reportSpam.RaiseCanExecuteChanged();
            manageLists.RaiseCanExecuteChanged();
            changeAvatar.RaiseCanExecuteChanged();
        }

        void ReceiveFriendshipInfo(TwitterFriendship friendship, TwitterResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageService.ShowWarning(Resources.CouldntGetRelationship);
                return;
            }

            Followed = friendship.Relationship.Source.Following;

            followUser.RaiseCanExecuteChanged();
            unfollowUser.RaiseCanExecuteChanged();
        }

        void ReceiveBlockedUsers(IEnumerable<int> blockedIds, TwitterResponse response)
        {
            if (blockedIds == null)
                blockedIds = new List<int>();

            Blocked = blockedIds.Any() && blockedIds.Contains(User.Id);

            block.RaiseCanExecuteChanged();
            unblock.RaiseCanExecuteChanged();
        }
    }
}
