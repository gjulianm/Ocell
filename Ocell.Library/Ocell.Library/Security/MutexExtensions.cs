using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Ocell.Library.Security
{
    public static class MutexUtil
    {
        // http://stackoverflow.com/a/229567
        public static bool DoWork(string name, Action f, bool throwOnError = false)
        {
            bool workDone = false;

            string mutexId = name;

            using (var mutex = new Mutex(false, mutexId))
            {
                var hasHandle = false;
                try
                {
                    hasHandle = mutex.WaitOne(5000);

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
                            mutex.ReleaseMutex();
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Couldn't release Mutex: {0}", e);
                        }
                    }
                }
            }

            return workDone;
        }
    }
}
