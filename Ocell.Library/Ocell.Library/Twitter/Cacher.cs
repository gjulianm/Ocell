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
            string invalid = ";:/@";
            string copy = "";

            // The alternative is using regexes, and I think this is more simple.
            foreach (var c in str)
                if (!invalid.Contains(c))
                    copy += c;

            return copy;
        }

        private static string GetCacheName(TwitterResource resource)
        {
            string Key = "Cache" + RemoveSymbols(resource.String);
            return Key;
        }

        private static TwitterStatus CorrectDateForSerialization(TwitterStatus status)
        {
            return status;
        }

        private static TwitterStatus CorrectDateForDeserialization(TwitterStatus status)
        {
            status.CreatedDate = status.CreatedDate.ToLocalTime();
            return status;
        }

        public static void SaveToCache(TwitterResource resource, IEnumerable<TwitterStatus> list)
        {
            string fileName = GetCacheName(resource);
            var fileManager = Dependency.Resolve<IFileManager>();
            var serializer = new SharpSerializer(SerializerSettings);

            MutexUtil.DoWork("OCELL_FILE_MUTEX" + fileName, () =>
            {
                try
                {
                    var toSave = list.Distinct().OfType<TwitterStatus>().Select(CorrectDateForSerialization).ToList();
                    using (var file = fileManager.OpenFile(fileName, FilePermissions.Write, FileOpenMode.Create))
                        serializer.Serialize(toSave, file.FileStream);

                    AncoraLogger.Instance.LogEvent("Saved " + toSave.Count.ToString() + " items to cache for resource " + resource.ToString(), AncoraMVVM.Base.Diagnostics.LogLevel.Message);
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

            statuses = statuses ?? new List<TwitterStatus>();

            return statuses.Select(CorrectDateForDeserialization).ToList();
        }

        public static void PreloadCache(TwitterResource resource)
        {
            memCache[resource] = GetFromCache(resource);
        }
    }
}