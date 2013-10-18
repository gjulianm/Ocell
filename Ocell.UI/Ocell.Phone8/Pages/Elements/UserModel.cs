using DanielVaughan.Windows;
using Microsoft.Phone.Tasks;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using TweetSharp;

namespace Ocell.Pages.Elements
{
    public class UserModel : ExtendedViewModelBase
    {
        public TwitterUser User { get; set; } // Not a property, don't need Assign().

        bool friendshipRetrieved;
        public bool FriendshipRetrieved
        {
            get { return friendshipRetrieved; }
            set { Assign("FriendshipRetrieved", ref friendshipRetrieved, value); }
        }

        bool followed;
        public bool Followed
        {
            get { return followed; }
            set { Assign("Followed", ref followed, value); }
        }

        bool followsMe;
        public bool FollowsMe
        {
            get { return followsMe; }
            set { Assign("FollowsMe", ref followsMe, value); }
        }

        string relationshipText;
        public string RelationshipText
        {
            get { return relationshipText; }
            set { Assign("RelationshipText", ref relationshipText, value); }
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
            User = null;
            GenericCanExecute = (obj) => User != null && DataTransfer.CurrentAccount != null;

            followUser = new DelegateCommand((obj) =>
            {
                IsLoading = true;
                ServiceDispatcher.GetService(DataTransfer.CurrentAccount).FollowUserAsync(new FollowUserOptions { UserId = User.Id }).ContinueWith(ReceiveFollow);
            }, x => FriendshipRetrieved && GenericCanExecute.Invoke(null));

            unfollowUser = new DelegateCommand((obj) =>
            {
                IsLoading = true;
                ServiceDispatcher.GetService(DataTransfer.CurrentAccount).UnfollowUserAsync(new UnfollowUserOptions { UserId = User.Id }).ContinueWith(ReceiveFollow);
            }, x => FriendshipRetrieved && GenericCanExecute.Invoke(null));

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
                    ServiceDispatcher.GetService(DataTransfer.CurrentAccount).BlockUserAsync(new BlockUserOptions { UserId = User.Id }).ContinueWith(ReceiveBlock);
                }, obj => GenericCanExecute(obj) && DataTransfer.CurrentAccount.ScreenName != User.ScreenName);

            unblock = new DelegateCommand((obj) =>
                {
                    IsLoading = true;
                    ServiceDispatcher.GetService(DataTransfer.CurrentAccount).UnblockUserAsync(new UnblockUserOptions { UserId = User.Id }).ContinueWith(ReceiveBlock);
                }, obj => GenericCanExecute(obj) && DataTransfer.CurrentAccount.ScreenName != User.ScreenName);

            reportSpam = new DelegateCommand(async (obj) =>
            {
                IsLoading = true;
                var response = await ServiceDispatcher.GetService(DataTransfer.CurrentAccount).ReportSpamAsync(new ReportSpamOptions { UserId = User.Id });
                IsLoading = false;

                if (response.StatusCode == HttpStatusCode.OK)
                    MessageService.ShowLightNotification(String.Format(Resources.ReportedAndBlocked, User.ScreenName));
                else
                    MessageService.ShowError(String.Format(Resources.CouldntReport, User.ScreenName));

            }, obj => GenericCanExecute(obj) && DataTransfer.CurrentAccount.ScreenName != User.ScreenName);

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

            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "FollowsMe")
                    UpdateRelationshipText();
                if (e.PropertyName == "ScreenName")
                    UpdateRelationshipText();
            };
        }

        private void UpdateRelationshipText()
        {
            if (!FriendshipRetrieved || IsOwner)
                RelationshipText = "";
            else if (FollowsMe)
                RelationshipText = String.Format(Resources.XFollowsY, ScreenName, DataTransfer.CurrentAccount.ScreenName);
            else
                RelationshipText = String.Format(Resources.XNotFollowsY, ScreenName, DataTransfer.CurrentAccount.ScreenName);
        }

        public void Loaded(string userName)
        {
            Regex remove = new Regex("@|:");
            userName = remove.Replace(userName, "");

            ScreenName = userName;

            GetUser(userName);           
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
                // TODO: When image uploads are ready.
                // srv.UpdateProfileImage(new UpdateProfileImageOptions { ImagePath = e.OriginalFileName }, ReceivePhotoUpload);
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

        void ReceiveFollow(Task<TwitterResponse<TwitterUser>> task)
        {
            var response = task.Result;

            var usr = response.Content;

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
            if (response.RequestSucceeded)
            {
                MessageService.ShowLightNotification(successMsg);
                Followed = !Followed;
                followUser.RaiseCanExecuteChanged();
                unfollowUser.RaiseCanExecuteChanged(); ;
            }
            else
                MessageService.ShowError(errorMsg);
        }

        void ReceiveBlock(Task<TwitterResponse<TwitterUser>> task)
        {
            var response = task.Result;

            string successMsg = "", errorMsg = "";

            if (!Blocked)
            {
                successMsg = String.Format(Resources.UserIsNowUnblocked, User.ScreenName);
                errorMsg = String.Format(Resources.CouldntUnblock, User.ScreenName);
            }
            else
            {
                successMsg = String.Format(Resources.UserIsNowBlocked, User.ScreenName);
                errorMsg = String.Format(Resources.CouldntBlock, User.ScreenName);
            }

            IsLoading = false;

            if (response.RequestSucceeded)
            {
                MessageService.ShowLightNotification(successMsg);
                Blocked = !Blocked;
                block.RaiseCanExecuteChanged();
                unblock.RaiseCanExecuteChanged(); // TODO: Implement AncoraMVVM here and avoid all those RaiseCanExecuteChanged.
            }
            else
                MessageService.ShowError(errorMsg);
        }

        async void GetUser(string userName)
        {
            BarText = Resources.RetrievingUser;
            IsLoading = true;

            var response = await ServiceDispatcher.GetDefaultService().ListUserProfilesForAsync(new ListUserProfilesForOptions { ScreenName = new List<string> { userName } });
            var users = response.Content;
            BarText = "";
            IsLoading = false;
            if (!response.RequestSucceeded|| !users.Any())
            {
                MessageService.ShowError(Resources.CouldntFindUser);
                return;
            }

            User = users.First();

            if (User.ProfileImageUrl != null)
                Avatar = User.ProfileImageUrl.Replace("_normal", "");

            Name = User.Name;
            ScreenName = User.ScreenName;
            Website = User.Url;
            Biography = User.Description;
            Tweets = User.StatusesCount.ToString();
            Followers = User.FollowersCount.ToString();
            Following = User.FriendsCount.ToString();
            WebsiteEnabled = Uri.IsWellFormedUriString(User.Url, UriKind.Absolute);
            IsOwner = Config.Accounts.Any(item => item.Id == User.Id);

            GetFriendshipInformation();


            // TODO: Come on.
            followUser.RaiseCanExecuteChanged();
            unfollowUser.RaiseCanExecuteChanged();
            block.RaiseCanExecuteChanged();
            unblock.RaiseCanExecuteChanged();
            pinUser.RaiseCanExecuteChanged();
            reportSpam.RaiseCanExecuteChanged();
            manageLists.RaiseCanExecuteChanged();
            changeAvatar.RaiseCanExecuteChanged();
        }

        private void GetFriendshipInformation()
        {
            if (DataTransfer.CurrentAccount != null)
            {
                var service = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);
                service.GetFriendshipInfoAsync(new GetFriendshipInfoOptions { SourceScreenName = DataTransfer.CurrentAccount.ScreenName, TargetScreenName = User.ScreenName }).ContinueWith(ReceiveFriendshipInfo);
                service.ListBlockedUserIdsAsync(new ListBlockedUserIdsOptions { Cursor = -1 }).ContinueWith(ReceiveBlockedUsers);
            }
        }

        void ReceiveFriendshipInfo(Task<TwitterResponse<TwitterFriendship>> task)
        {
            var response = task.Result;

            FriendshipRetrieved = true;
            if (!response.RequestSucceeded)
            {
                MessageService.ShowWarning(Resources.CouldntGetRelationship);
                return;
            }

            var friendship = response.Content;

            Followed = friendship.Relationship.Source.Following;
            FollowsMe = friendship.Relationship.Source.FollowedBy;
            UpdateRelationshipText();
            followUser.RaiseCanExecuteChanged();
            unfollowUser.RaiseCanExecuteChanged();
        }

        void ReceiveBlockedUsers(Task<TwitterResponse<TwitterCursorList<long>>> task)
        {
            IEnumerable<long> blockedIds = task.Result.Content;

            if (blockedIds == null)
                blockedIds = new List<long>();

            Blocked = blockedIds.Any() && blockedIds.Contains(User.Id);

            block.RaiseCanExecuteChanged();
            unblock.RaiseCanExecuteChanged();
        }
    }
}
