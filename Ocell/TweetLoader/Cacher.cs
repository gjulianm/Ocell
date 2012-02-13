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

        private static IEnumerable<string> ReadContentsOf(string filename)
        {
            IsolatedStorageFile Storage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream File = Storage.OpenFile(filename, System.IO.FileMode.OpenOrCreate);
            char separator = char.MaxValue;
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            List<string> Strings = new List<string>();

            string contents;
            byte[] bytes= new byte[File.Length];
            string line = "";
            int NewlineIndex;

            File.Read(bytes, 0, (int)File.Length);
            contents = new string(encoding.GetChars(bytes));

            while ((NewlineIndex = contents.IndexOf(separator)) != -1)
            {
                line = contents.Substring(0, NewlineIndex);
                contents = contents.Substring(NewlineIndex + 1);
                Strings.Add(line);
            }

            return Strings;
        }

        private static void SaveContentsIn(string filename, IEnumerable<string> strings)
        {
            IsolatedStorageFile Storage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream File = Storage.OpenFile(filename, System.IO.FileMode.Create);
            char separator = char.MaxValue;
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            string contents = "";
            byte[] bytes;


            foreach (var str in strings)
                contents += str + separator;

            bytes = encoding.GetBytes(contents);
            File.Write(bytes, 0, bytes.Length);
            File.Close();
        }

        public static void SaveToCache(TwitterResource Resource, IEnumerable<TwitterStatus> List)
        {
            IsolatedStorageSettings Config = IsolatedStorageSettings.ApplicationSettings;
            string Key = GetCacheName(Resource);
            List<string> Strings = new List<string>();
            

            foreach (ITweetable Item in List)
                Strings.Add(Item.RawSource);
            try
            {
                SaveContentsIn(Key, Strings);
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
                Strings = new List<string>(ReadContentsOf(Key));
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
