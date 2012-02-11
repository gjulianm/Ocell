using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using TweetSharp;
namespace Ocell
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

        private static string GetCacheName(TwitterResource Resource)
        {
            string Key = "Cache" + RemoveSymbols(Resource.String);
            return Key;
        }

        public static void SaveToCache(TwitterResource Resource, IEnumerable<TwitterStatus> List)
        {
            IsolatedStorageSettings Config = IsolatedStorageSettings.ApplicationSettings;
            string Key = GetCacheName(Resource);
            List<string> Strings = new List<string>();
            

            if (Config.Contains(Key))
                Config.Remove(Key);

            foreach (ITweetable Item in List)
                Strings.Add(Item.RawSource);
            try
            {
                Config.Add(Key, Strings);
                Config.Save();
            }
            catch (Exception)
            {
            }
        }

        public static IEnumerable<TwitterStatus> GetFromCache(TwitterResource Resource)
        {
            IsolatedStorageSettings Config = IsolatedStorageSettings.ApplicationSettings;
            List<string> Strings;
            List<TwitterStatus> List = new List<TwitterStatus>();
            string Key = GetCacheName(Resource);
            TwitterStatus Item;

            try 
            {
                if(!Config.TryGetValue<List<string>>(Key, out Strings))
                    return List;
            }
            catch(Exception)
            {
                return List;
            }

            foreach(string Raw in Strings)
            {
                Item = ServiceDispatcher.GetDefaultService().Deserialize<TwitterStatus>(Raw);
                List.Add(Item);
            }

            return List;

        }
    }
}
