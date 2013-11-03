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

            foreach (var tweet in list)
                tweet.CreatedDate = new DateTime(tweet.CreatedDate.Ticks, DateTimeKind.Local);

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
            var stopwatch = new Stopwatch();
            long partial0 = -1, partial2 = -1, partial3 = -1;
            stopwatch.Start();
            partial0 = stopwatch.ElapsedMilliseconds;
            string fileName = GetCacheName(resource);
            var serializer = new SharpSerializer(SerializerSettings);

            IEnumerable<TwitterStatus> statuses = null;

            var partial1 = stopwatch.ElapsedMilliseconds;

            MutexUtil.DoWork("OCELL_FILE_MUTEX" + fileName, () =>
            {
                partial2 = stopwatch.ElapsedMilliseconds;
                try
                {
                    using (var stream = FileAbstractor.GetFileStream(fileName))
                    {
                        partial3 = stopwatch.ElapsedMilliseconds;
                        if (stream.Length != 0)
                            statuses = serializer.Deserialize(stream) as IEnumerable<TwitterStatus>;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Trace(ex.ToString());
                }
            });
            stopwatch.Stop();

            Logger.Trace(String.Format("GetFromCache: Partial0 {0}ms, Partial1 {2}ms, Partial2 {2}ms, Partial3 {3}ms, Final {4}ms.",
                partial0, partial1, partial2, partial3, stopwatch.ElapsedMilliseconds));

            return statuses ?? new List<TwitterStatus>();
        }
    }
}