using AncoraMVVM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public class UsernameProvider
    {
        private static Dictionary<UserToken, IList<string>> dicUsers = new Dictionary<UserToken, IList<string>>();
        private static Dictionary<UserToken, bool> finishedUsers = new Dictionary<UserToken, bool>();

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

        public static void FillUserNames(IEnumerable<UserToken> users)
        {
            foreach (var user in users)
            {
                var temp = user;
                finishedUsers[temp] = false;
                dicUsers[temp] = GetUserCache(temp).ToList();
                FillUserNamesFor(temp, -1);
            }
        }

        protected static async void FillUserNamesFor(UserToken user, long cursor)
        {
            var response = await ServiceDispatcher.GetService(user).ListFriendsAsync(new ListFriendsOptions { ScreenName = user.ScreenName, Cursor = cursor });

            if (!response.RequestSucceeded)
                return;

            var friends = response.Content;

            if (dicUsers.ContainsKey(user))
                dicUsers[user] = dicUsers[user].Union(friends.Select(x => x.ScreenName)).ToList();
            else
                dicUsers[user] = friends.Select(x => x.ScreenName).ToList();

            if (friends.NextCursor != null && friends.NextCursor != 0)
            {
                FillUserNamesFor(user, (long)friends.NextCursor);
            }
            else
            {
                finishedUsers[user] = true;
                SaveUserCache(user, dicUsers[user]);
            }
        }

        public UsernameProvider()
        {
        }

        public void Start()
        {
            bool userFinished;
            if (!finishedUsers.TryGetValue(User, out userFinished) || !userFinished)
                UsernameProvider.FillUserNames(new List<UserToken> { User });
        }

        private static IEnumerable<string> GetUserCache(UserToken user)
        {
            string filename = "AUTOCOMPLETECACHE" + user.ScreenName;
            return FileAbstractor.ReadLinesOfFile(filename);
        }

        private static void SaveUserCache(UserToken user, IEnumerable<string> names)
        {
            string filename = "AUTOCOMPLETECACHE" + user.ScreenName;
            FileAbstractor.WriteLinesToFile(names, filename);
        }

        public event OnError Error;
    }
}
