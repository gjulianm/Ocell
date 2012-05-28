using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using TweetSharp;

namespace Ocell.Library.Twitter
{
    public class UserProvider : IUserProvider
    {
        public SafeObservable<TwitterUser> Users { get; set; }
        public UserToken User { get; set; }
        public bool GetFollowers { get; set; }
        public bool GetFollowing { get; set; }
        private TwitterService _service;

        public UserProvider()
        {
            GetFollowers = true;
            GetFollowing = true;
            Users = new SafeObservable<TwitterUser>();
        }

        private void GetService()
        {
            if (User == null)
                _service = ServiceDispatcher.GetDefaultService();
            else
                _service = ServiceDispatcher.GetService(User);
        }

        public void Start()
        {
            if (_service == null)
                GetService();

            if(GetFollowing)
                _service.ListFriends(-1, ReceiveList);
            if (GetFollowers)
                _service.ListFollowers(-1, ReceiveList);
        }

        private void ReceiveList(TwitterCursorList<TwitterUser> users, TwitterResponse response)
        {
            bool finish = false;
            if (users == null || response.StatusCode != HttpStatusCode.OK)
            {
                if (Error != null)
                    Error(this, response);
            }

            if (users.NextCursor != null && users.NextCursor != 0 && _service != null)
            {
                if (GetFollowing)
                    _service.ListFriends((long)users.NextCursor, ReceiveList);
                if (GetFollowers)
                    _service.ListFollowers((long)users.NextCursor, ReceiveList);
            }
            else
                finish = true;

            if (Users == null)
                Users = new SafeObservable<TwitterUser>();

            foreach (var user in users)
                if(!Users.Contains(user))
                    Users.Add(user);

            if (finish && Finished != null)
                Finished(this, new EventArgs());
        }

        public event OnError Error;

        public event EventHandler Finished;
    }

    public class UsernameProvider
    {
        public IList<string> Usernames { get; protected set; }
        public UserToken User { get; set; }
        private TwitterService _service;
        private bool _stop;

        public UsernameProvider()
        {
            Usernames = new List<string>();
            _stop = false;
        }

        private void GetService()
        {
            if (User == null)
                _service = ServiceDispatcher.GetDefaultService();
            else
                _service = ServiceDispatcher.GetService(User);
        }

        public void Start()
        {
            if (_service == null)
                GetService();

            _service.ListFriends(-1, ReceiveFriends);
        }

        private void ReceiveFriends(TwitterCursorList<TwitterUser> friends, TwitterResponse response)
        {
            if (friends == null || response.StatusCode != HttpStatusCode.OK)
            {
                if (Error != null)
                    Error(this, response);
                return;
            }

            if (_stop)
            {
                _stop = false;
                return;
            }

            if (friends.NextCursor != null && friends.NextCursor != 0 && _service != null)
                _service.ListFriends((long)friends.NextCursor, ReceiveFriends);

            if (Usernames == null)
                Usernames = new List<string>();

            foreach (var user in friends)
                Usernames.Add(user.ScreenName);
        }

        public void Stop()
        {
            _stop = true;
        }

        public event OnError Error;
    }
}
