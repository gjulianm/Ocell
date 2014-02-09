using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Ocell.Library.Security
{
    public static class MutexUtil
    {
        private static Dictionary<string, object> syncObjects = new Dictionary<string, object>();
        private static object dicSync = new object();
        // http://stackoverflow.com/a/229567
        public static bool DoWork(string name, Action f, bool throwOnError = false)
        {
            bool workDone = false;

            string mutexId = name;

            object sync;

            lock (dicSync)
            {
                if (!syncObjects.TryGetValue(name, out sync))
                {
                    sync = new object();
                    syncObjects[name] = sync;
                }
            }

            var hasHandle = false;
            try
            {

                hasHandle = Monitor.TryEnter(sync, 5000);

                if (hasHandle)
                {
                    f();
                    workDone = true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error working with Mutex: {0}", e.Message);
                if (throwOnError)
                    throw;
            }
            finally
            {
                if (hasHandle)
                {
                    try
                    {
                        Monitor.Exit(sync);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Couldn't release Mutex: {0}", e);
                    }
                }
            }

            return workDone;
        }
    }
}
