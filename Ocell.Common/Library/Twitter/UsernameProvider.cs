using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using TweetSharp;
using System.Linq;
using System.IO;
using Ocell.Library;

namespace Ocell.Library.Twitter
{
    public class UserProvider : IUserProvider
    {
        public SafeObservable<TwitterUser> Users { get; set; }
        public UserToken User { get; set; }
        public bool GetFollowers { get; set; }
        public bool GetFollowing { get; set; }
        private ITwitterService _service;

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
                finish = true;
                return;
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
        private static Dictionary<UserToken, IList<string>> dicUsers = new Dictionary<UserToken,IList<string>>();
        private static Dictionary<UserToken, bool> finishedUsers = new Dictionary<UserToken,bool>();

        public IList<string> Usernames
        {
            get
            {
                IList<string> list;
                if (dicUsers.TryGetValue(User, out list))
                    return list;
                else
                    return new List<string>();
            }
        }

        public UserToken User { get; set; }
        private bool _stop;

        public static void FillUserNames(IEnumerable<UserToken> users)
        {
            foreach (var user in users)
            {
                var temp = user;
                finishedUsers[temp] = false;
                dicUsers[temp] = GetUserCache(temp).ToList();
                ServiceDispatcher.GetService(temp).ListFriends(-1, (list, response) => ReceiveFriends(list, response, temp));
            }
        }

        public UsernameProvider()
        {
            _stop = false;
        }

        public void Start()
        {
            bool userFinished;
            if (!finishedUsers.TryGetValue(User, out userFinished) || !userFinished)
                UsernameProvider.FillUserNames(new List<UserToken> { User });
        }

        private static void ReceiveFriends(TwitterCursorList<TwitterUser> friends, TwitterResponse response, UserToken user)
        {
            if (friends == null || response.StatusCode != HttpStatusCode.OK)     
                return;

            if (dicUsers.ContainsKey(user))
                dicUsers[user] = dicUsers[user].Union(friends.Select(x => x.ScreenName)).ToList();
            else
                dicUsers[user] = friends.Select(x => x.ScreenName).ToList();

            if (friends.NextCursor != null && friends.NextCursor != 0)
                ServiceDispatcher.GetService(user).ListFriends((long)friends.NextCursor, (l, r) => ReceiveFriends(l, r, user));
            else
            {
                finishedUsers[user] = true;
                SaveUserCache(user, dicUsers[user]);
            }

            
        }

        public void Stop()
        {
            _stop = true;
        }

        private static IEnumerable<string> GetUserCache(UserToken user)
        {
            string filename = "AUTOCOMPLETECACHE" + user.ScreenName;
            var list = FileAbstractor.ReadLinesOfFile(filename).ToList();
            return list;
        }

        private static void SaveUserCache(UserToken user, IEnumerable<string> names)
        {
            string filename = "AUTOCOMPLETECACHE" + user.ScreenName;
            FileAbstractor.WriteLinesToFile(names, filename);
        }

        public event OnError Error;
    }
}
