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
using Polenter.Serialization;
using System.Text;
using Ocell.Library.Security;

namespace Ocell.Library.Twitter
{
    public static class Cacher
    {
        static SharpSerializerBinarySettings SerializerSettings = new SharpSerializerBinarySettings
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
                    Debug.WriteLine(ex);
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
