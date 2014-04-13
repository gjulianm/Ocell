using AncoraMVVM.Base;
using AncoraMVVM.Base.Diagnostics;
using AncoraMVVM.Base.Files;
using AncoraMVVM.Base.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetSharp;

namespace Ocell.Library.Twitter
{
    public class UsernameProvider : IDataProvider<string>
    {
        private static bool downloadStarted = false;
        private static SafeObservable<string> userCache = new SafeObservable<string>();
        public SafeObservable<string> DataList { get { return userCache; } }
        public IEnumerable<UserToken> Users { get; private set; }

        public static async Task DownloadAndCacheFriends(IEnumerable<UserToken> users)
        {
            var cacheTask = GetUserCache();
            var tasks = users.Select(user => DownloadUsernamesFor(user, -1));
            await TaskEx.WhenAll(tasks);
            await cacheTask;
        }

        protected static async Task DownloadUsernamesFor(UserToken user, long cursor = -1)
        {
            TwitterResponse<TwitterCursorList<TwitterUser>> response;

            try
            {
                response = await ServiceDispatcher.GetService(user).ListFriendsAsync(new ListFriendsOptions { ScreenName = user.ScreenName, Cursor = cursor });
            }
            catch (Exception ex)
            {
                AncoraLogger.Instance.LogException("Web exception trying to retrieve usernames.", ex);
                return;
            }

            if (!response.RequestSucceeded)
                return;

            var friends = response.Content;

            userCache.AddListRange(friends.Select(x => x.ScreenName).Except(userCache));

            if (friends.NextCursor != null && friends.NextCursor != 0)
                await DownloadUsernamesFor(user, (long)friends.NextCursor);
            else
                await SaveUserCache(userCache);
        }

        public UsernameProvider(IEnumerable<UserToken> users)
        {
            Users = users.ToList();
        }

        public async void StartRetrieval()
        {
            if (!downloadStarted)
                await DownloadAndCacheFriends(Users);
        }

        private async static Task<IEnumerable<string>> GetUserCache()
        {
            string filename = "AUTOCOMPLETECACHE";
            return await Dependency.Resolve<IFileManager>().ReadLines(filename);
        }

        private async static Task SaveUserCache(IEnumerable<string> names)
        {
            string filename = "AUTOCOMPLETECACHE";
            await Dependency.Resolve<IFileManager>().WriteLines(filename, names);
        }
    }
}
