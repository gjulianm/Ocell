using Ocell.Library.Twitter.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;
using TweetSharp;

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

        public static void SaveToCache(TwitterResource Resource, IEnumerable<TwitterStatus> List)
        {
            _saveToCache(Resource, List);
        }

        private static void _saveToCache(TwitterResource Resource, IEnumerable<TwitterStatus> List)
        {
            string key = GetCacheName(Resource);
            List<string> Strings = new List<string>();

			List = List.Distinct();
						
            try
            {
                foreach (ITweetable Item in List)
                    Strings.Add(Item.RawSource);
            }
            catch (InvalidCastException)
            {
                // Just stop adding strings when we encounter a non-TwitterStatus element.
            }
	

            try
            {
                FileAbstractor.WriteBlocksToFile(Strings, key);
            }
            catch (Exception)
            {
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
            List<TwitterStatus> List = new List<TwitterStatus>();
            IEnumerable<string> Strings;

            string Key = GetCacheName(Resource);
            TwitterStatus Item;
            TwitterService DefaultService = new TwitterService();

            try
            {
                Strings = FileAbstractor.ReadBlocksOfFile(Key);
            }
            catch (Exception)
            {
                return new List<TwitterStatus>();
            }

            foreach (string Raw in Strings)
            {
                try
                {
                    Item = DefaultService.Deserialize<TwitterStatus>(Raw);
                    List.Add(Item);
                }
                catch (Exception)
                {
                }
            }

            return List.OrderByDescending(item => item.Id); ;
        }
    }
}
