using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using TweetSharp;
using System.Linq;
using Ocell.Library.Twitter.Comparers;
using System.Threading;

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

        private static IEnumerable<string> ReadContentsOf(string filename)
        {
            IEnumerable<string> List = null;

            using (var mutex = new Mutex(false, "Ocell" + filename))
            {
                if (!mutex.WaitOne(100))
                    return new List<string>();

                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                using (IsolatedStorageFileStream file = storage.OpenFile(filename, System.IO.FileMode.OpenOrCreate))
                {

                    try
                    {
                        List = new List<string>(file.ReadLines()); // We have to create the list now to avoid reading it when the file is closed.
                        file.Close();
                        return List;
                    }
                    catch (Exception)
                    {
                        return new List<string>();
                    }
                }
            }
        }

        private static void SaveContentsIn(string filename, IEnumerable<string> strings, System.IO.FileMode Mode)
        {
            using (var mutex = new Mutex(false, "Ocell" + filename))
            {
                if (!mutex.WaitOne(100))
                    return;

                IsolatedStorageFile Storage = IsolatedStorageFile.GetUserStoreForApplication();
                try
                {
                    IsolatedStorageFileStream File = Storage.OpenFile(filename, Mode);
                    File.WriteLines(strings);
                    File.Close();
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        public static void SaveToCache(TwitterResource Resource, IEnumerable<TwitterStatus> List)
        {
            _saveToCache(Resource, List, System.IO.FileMode.Create);
        }

        private static void _saveToCache(TwitterResource Resource, IEnumerable<TwitterStatus> List, System.IO.FileMode Mode)
        {
            IsolatedStorageSettings Config = IsolatedStorageSettings.ApplicationSettings;
            string Key = GetCacheName(Resource);
            List<string> Strings = new List<string>();


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
                SaveContentsIn(Key, Strings, Mode);
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
            IsolatedStorageSettings Config = IsolatedStorageSettings.ApplicationSettings;
            IEnumerable<string> Strings;

            string Key = GetCacheName(Resource);
            TwitterStatus Item;
            TwitterService DefaultService = new TwitterService();

            try
            {
                Strings = ReadContentsOf(Key);
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
