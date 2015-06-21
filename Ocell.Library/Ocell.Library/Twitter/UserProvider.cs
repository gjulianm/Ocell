using System;
using System.Threading.Tasks;
using AncoraMVVM.Base;
using TweetSharp;

namespace Ocell.Library.Twitter
{
    public class UserProvider : IUserProvider
    {
        public SafeObservable<TwitterUser> Users { get; set; }
        public UserToken User { get; set; }
        public bool GetFollowers { get; set; }
        public bool GetFollowing { get; set; }
        public bool DownloadingFollowers { get; set; }
        public bool DownloadingFollowing { get; set; }
        public bool Downloading { get { return DownloadingFollowers || DownloadingFollowing; } }

        private ITwitterService service;

        public UserProvider()
        {
            GetFollowers = true;
            GetFollowing = true;
            Users = new SafeObservable<TwitterUser>();
            DownloadingFollowing = false;
            DownloadingFollowers = false;
        }

        private void CreateService()
        {
            service = User == null ? ServiceDispatcher.GetDefaultService() : ServiceDispatcher.GetService(User);
        }

        public void Start()
        {
            if (service == null)
                CreateService();

            GetUsers(-1);
        }

        private void GetUsers(long cursor)
        {
            if (GetFollowing)
                GetFollowingUsers(cursor);

            if (GetFollowers)
                GetFollowerUsers(cursor);
        }

        private void GetFollowerUsers(long cursor)
        {
            service.ListFollowersAsync(new ListFollowersOptions { ScreenName = User.ScreenName, Cursor = cursor })
                .ContinueWith(ReceiveFollowerList);
            DownloadingFollowers = true;
        }

        private void ReceiveFollowerList(Task<TwitterResponse<TwitterCursorList<TwitterUser>>> task)
        {
            long? cursor = ReceiveList(task);

            if (cursor == null)
            {
                DownloadingFollowers = false;
                OnDownloadFinalization();
            }
            else
                GetFollowerUsers(cursor.Value);
        }

        private void OnDownloadFinalization()
        {
            if (!Downloading && Finished != null)
                Finished(this, new EventArgs());
        }

        private void GetFollowingUsers(long cursor)
        {
            service.ListFriendsAsync(new ListFriendsOptions { ScreenName = User.ScreenName, Cursor = cursor })
                .ContinueWith(ReceiveFollowingList);
            DownloadingFollowing = true;
        }

        private void ReceiveFollowingList(Task<TwitterResponse<TwitterCursorList<TwitterUser>>> task)
        {
            long? cursor = ReceiveList(task);

            if (cursor == null)
            {
                DownloadingFollowing = false;
                OnDownloadFinalization();
            }
            else
                GetFollowingUsers(cursor.Value);
        }


        private long? ReceiveList(Task<TwitterResponse<TwitterCursorList<TwitterUser>>> task)
        {
            long? nextCursor = null;
            var response = task.Result;

            if (!response.RequestSucceeded)
            {
                if (Error != null)
                    Error(this, response);

                return null;
            }

            var users = response.Content;

            if (users.NextCursor != null && users.NextCursor != 0)
                nextCursor = users.NextCursor;

            foreach (var user in users)
                if (!Users.Contains(user))
                    Users.Add(user);

            return nextCursor;
        }

        public event OnError Error;

        public event EventHandler Finished;
    }
}
