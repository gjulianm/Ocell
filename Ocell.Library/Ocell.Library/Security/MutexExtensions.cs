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

            // get application GUID as defined in AssemblyInfo.cs
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();

            // unique id for global mutex - Global prefix means it is global to the machine
            string mutexId = string.Format("{1} Global\\{{{0}}}", appGuid, name);

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
                        mutex.ReleaseMutex();
                }
            }

            return workDone;
        }
    }
}
