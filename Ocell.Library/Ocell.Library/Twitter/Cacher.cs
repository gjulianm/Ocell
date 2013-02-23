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
            Mutex mutex = new Mutex(false, "OCELL_FILE_MUTEX" + fileName);

            if (mutex.WaitOne(1000))
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
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        public static IEnumerable<TwitterStatus> GetFromCache(TwitterResource resource)
        {
            string fileName = GetCacheName(resource);
            var serializer = new SharpSerializer(SerializerSettings);
            Mutex mutex = new Mutex(false, "OCELL_FILE_MUTEX" + fileName);
            IEnumerable<TwitterStatus> statuses = null;

            if (mutex.WaitOne(1000))
            {
                try
                {
                    using (var stream = FileAbstractor.GetFileStream(fileName))
                    {
                        if(stream.Length != 0)
                            statuses = serializer.Deserialize(stream) as IEnumerable<TwitterStatus>;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }

            return statuses ?? new List<TwitterStatus>();
        }
    }
}
