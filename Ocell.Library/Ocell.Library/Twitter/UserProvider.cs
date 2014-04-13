using AncoraMVVM.Base;
using System;
using System.Threading.Tasks;
using TweetSharp;

namespace Ocell.Library.Twitter
{
    public class UserProvider : IUserProvider
    {
        public SafeObservable<TwitterUser> Users { get; set; }
        public UserToken User { get; set; }
        public bool GetFollowers { get; set; }
        public bool GetFollowing { get; set; }
        private ITwitterService service;

        public UserProvider()
        {
            GetFollowers = true;
            GetFollowing = true;
            Users = new SafeObservable<TwitterUser>();
        }

        private void GetService()
        {
            if (User == null)
                service = ServiceDispatcher.GetDefaultService();
            else
                service = ServiceDispatcher.GetService(User);
        }

        public void Start()
        {
            if (service == null)
                GetService();

            GetUsers(-1);
        }

        private void GetUsers(long cursor)
        {
            if (GetFollowing)
                service.ListFriendsAsync(new ListFriendsOptions { ScreenName = User.ScreenName, Cursor = cursor }).ContinueWith(ReceiveList);
            if (GetFollowers)
                service.ListFollowersAsync(new ListFollowersOptions { ScreenName = User.ScreenName, Cursor = cursor }).ContinueWith(ReceiveList);
        }

        private void ReceiveList(Task<TwitterResponse<TwitterCursorList<TwitterUser>>> task)
        {
            var response = task.Result;

            bool finished = false;

            if (!response.RequestSucceeded)
            {
                if (Error != null)
                    Error(this, response);

                return;
            }

            var users = response.Content;

            if (users.NextCursor != null && users.NextCursor != 0 && service != null)
                GetUsers((long)users.NextCursor); // TODO: This is not efficient if we are searching for both following and followers.
            else
                finished = true;

            if (Users == null)
                Users = new SafeObservable<TwitterUser>();

            foreach (var user in users)
                if (!Users.Contains(user))
                    Users.Add(user);

            if (finished && Finished != null)
                Finished(this, new EventArgs());
        }

        public event OnError Error;

        public event EventHandler Finished;
    }
}
