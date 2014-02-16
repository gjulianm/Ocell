using AncoraMVVM.Base.Diagnostics;
using AncoraMVVM.Base.Files;
using AncoraMVVM.Base.IoC;
using Ocell.Library.Security;
using Polenter.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using TweetSharp;

namespace Ocell.Library.Twitter
{
    public static class Cacher
    {
        private static Dictionary<TwitterResource, IEnumerable<TwitterStatus>> memCache = new Dictionary<TwitterResource, IEnumerable<TwitterStatus>>();

        private static SharpSerializerBinarySettings SerializerSettings = new SharpSerializerBinarySettings
        {
            Mode = BinarySerializationMode.SizeOptimized
        };

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
            string fileName = GetCacheName(resource);

            var fileManager = Dependency.Resolve<IFileManager>();

            foreach (var tweet in list)
                tweet.CreatedDate = tweet.CreatedDate.ToLocalTime();

            var serializer = new SharpSerializer(SerializerSettings);
            MutexUtil.DoWork("OCELL_FILE_MUTEX" + fileName, () =>
            {
                try
                {
                    using (var file = fileManager.OpenFile(fileName, FilePermissions.Write, FileOpenMode.Create))
                    {
                        serializer.Serialize(list.Distinct().OfType<TwitterStatus>().ToList(), file.FileStream);
                    }
                }
                catch (Exception ex)
                {
                    AncoraLogger.Instance.LogException("Error saving cache for " + resource.ToString(), ex);
                }
            });
        }

        public static IEnumerable<TwitterStatus> GetFromCache(TwitterResource resource)
        {
            IEnumerable<TwitterStatus> statuses = null;

            if (memCache.TryGetValue(resource, out statuses))
                return statuses;

            string fileName = GetCacheName(resource);
            var serializer = new SharpSerializer(SerializerSettings);
            var fileManager = Dependency.Resolve<IFileManager>();
            

            MutexUtil.DoWork("OCELL_FILE_MUTEX" + fileName, () =>
            {
                try
                {
                    using (var file = fileManager.OpenFile(fileName, FilePermissions.Read, FileOpenMode.OpenOrCreate))
                    {
                        if (file.FileStream.Length != 0)
                            statuses = serializer.Deserialize(file.FileStream) as IEnumerable<TwitterStatus>;
                    }
                }
                catch (Exception ex)
                {
                    AncoraLogger.Instance.LogException("Error getting cache from " + resource.ToString(), ex);
                }
            });

            return statuses ?? new List<TwitterStatus>();
        }

        public static void PreloadCache(TwitterResource resource)
        {
            memCache[resource] = GetFromCache(resource);
        }
    }
}