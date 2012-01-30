using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using TweetSharp;
namespace Ocell
{
    public static class Cacher
    {        
        public static void SaveToCache(TwitterResource Resource, IEnumerable<TwitterStatus> List)
        {
            IsolatedStorageSettings Config = IsolatedStorageSettings.ApplicationSettings;
            ListConverter Converter = new ListConverter();
            List<string> Strings = new List<string>();
            string Key = "Cache" + Converter.Convert(Resource.String, null, null, null);

            if (Config.Contains(Key))
                Config.Remove(Key);

            foreach (ITweetable Item in List)
                Strings.Add(Item.RawSource);

            Config.Add(Key, Strings);
            Config.Save();
        }

        public static IEnumerable<TwitterStatus> GetFromCache(TwitterResource Resource)
        {
            IsolatedStorageSettings Config = IsolatedStorageSettings.ApplicationSettings;
            ListConverter Converter = new ListConverter();
            List<string> Strings;
            List<TwitterStatus> List = new List<TwitterStatus>();
            string Key = "Cache" + Converter.Convert(Resource.String, null, null, null);
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
                Item = Clients.Service.Deserialize<TwitterStatus>(Raw);
                List.Add(Item);
            }

            return List;

        }
    }
}
