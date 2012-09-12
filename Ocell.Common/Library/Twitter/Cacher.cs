using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using TweetSharp;
using System.Linq;
using Ocell.Library.Twitter.Comparers;
using System.Threading;
using System.Diagnostics;

namespace Ocell.Library.Twitter
{
    public static class Cacher
    {
        private static string RemoveSymbols(string str)
        {
            string copy = "";
            foreach (char c in str)
                if (c != ';' && c != ':' && c != '/' && c != '@')
                    copy += c;

            return copy;
        }

        private static string GetCacheName(TwitterResource resource)
        {
            string Key = "Cache" + RemoveSymbols(resource.String);
            return Key;
        }

        public static void SaveToCache(TwitterResource resource, IEnumerable<TwitterStatus> list)
        {
            IsolatedStorageSettings Config = IsolatedStorageSettings.ApplicationSettings;
            string fileName = GetCacheName(resource);
            IEnumerable<string> strings;

            try
            {
                strings = list.Distinct().OfType<TwitterStatus>().Select(item => item.RawSource);
                FileAbstractor.WriteBlocksToFile(strings, fileName);
            }
            catch (InvalidCastException)
            {
                // Just stop adding strings when we encounter a non-TwitterStatus element.
            }
        }

        public static void AppendToCache(TwitterResource Resource, IEnumerable<TwitterStatus> List)
        {
            IEnumerable<TwitterStatus> CurrentCache = GetFromCache(Resource);
            TwitterStatusEqualityComparer Comparer = new TwitterStatusEqualityComparer();
            CurrentCache = CurrentCache.Concat(List).OrderBy(item => item.Id).Distinct(Comparer);
            SaveToCache(Resource, CurrentCache);
        }

        public static IEnumerable<TwitterStatus> GetFromCache(TwitterResource Resource)
        {
            var track = TimeTracker.StartTrack();
            List<TwitterStatus> list = new List<TwitterStatus>();
            IEnumerable<string> strings;

            string fileName = GetCacheName(Resource);
            TwitterStatus item = default(TwitterStatus);
            TwitterService DefaultService = new TwitterService();
            Stopwatch watch = new Stopwatch();

            try
            {
                var t1 = TimeTracker.StartTrack();
                strings = FileAbstractor.ReadBlocksOfFile(fileName);
                TimeTracker.EndTrack(t1, "ReadBlocksOfFile");
            }
            catch (Exception)
            {
                yield break;
            }

            var t2 = TimeTracker.StartTrack();
            foreach (string rawSource in strings)
            {
                bool deserializeSuccess = true;
                try
                {
                    item = DefaultService.Deserialize<TwitterStatus>(rawSource);
                }
                catch (Exception)
                {
                    deserializeSuccess = false;
                }

                if (deserializeSuccess)
                    yield return item;
            }
            TimeTracker.EndTrack(t2, "Deserialization");

            TimeTracker.EndTrack(track, "GetCache");
        }
    }
}
