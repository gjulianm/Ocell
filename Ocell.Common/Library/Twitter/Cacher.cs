using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using TweetSharp;
using System.Linq;
using Ocell.Library.Twitter.Comparers;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;

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
            var serializer = new JsonSerializer();

            try
            {
                using (var stream = new StringWriter())
                {
                    serializer.Serialize(stream, list.Distinct().OfType<TwitterStatus>());
                    FileAbstractor.WriteContentsToFile(stream.ToString(), fileName);
                }                
            }
            catch (Exception)
            {
            }            
        }

        public static IEnumerable<TwitterStatus> GetFromCache(TwitterResource Resource)
        {
            string fileName = GetCacheName(Resource);
            var serializer = new JsonSerializer();
            string contents = FileAbstractor.ReadContentsOfFile(fileName);
            IEnumerable<TwitterStatus> statuses;

            try
            {
                using (var stream = new StringReader(contents))
                {
                    using (var reader = new JsonTextReader(stream))
                    {
                        statuses = serializer.Deserialize<IEnumerable<TwitterStatus>>(reader);
                    }
                }
            }
            catch (Exception)
            {
                return new List<TwitterStatus>();
            }

            return statuses ?? new List<TwitterStatus>();
        }
    }
}
