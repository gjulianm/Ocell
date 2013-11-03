using Ocell.Library.Security;
using Polenter.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TweetSharp;

namespace Ocell.Library.Twitter
{
    public static class Cacher
    {
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

            var serializer = new SharpSerializer(SerializerSettings);
            MutexUtil.DoWork("OCELL_FILE_MUTEX" + fileName, () =>
            {
                try
                {
                    using (var stream = FileAbstractor.GetFileStream(fileName))
                    {
                        serializer.Serialize(list.Distinct().OfType<TwitterStatus>().ToList(), stream);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Trace(ex.ToString());
                }
            });
        }

        public static IEnumerable<TwitterStatus> GetFromCache(TwitterResource resource)
        {
            string fileName = GetCacheName(resource);
            var serializer = new SharpSerializer(SerializerSettings);

            IEnumerable<TwitterStatus> statuses = null;

            MutexUtil.DoWork("OCELL_FILE_MUTEX" + fileName, () =>
            {
                try
                {
                    using (var stream = FileAbstractor.GetFileStream(fileName))
                    {
                        if (stream.Length != 0)
                            statuses = serializer.Deserialize(stream) as IEnumerable<TwitterStatus>;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            });

            return statuses ?? new List<TwitterStatus>();
        }
    }
}