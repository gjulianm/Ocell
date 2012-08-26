using System;
using System.Collections.Generic;
using System.Threading;

#if METRO
using Windows.Storage;
using System.Threading.Tasks;
#else
#endif


namespace Ocell.Library
{
    public static class FileAbstractor
    {

#if METRO
        public static async void WriteContentsToFile(string contents, string fileName)
#else
        public static void WriteContentsToFile(string contents, string file)
#endif
        {
            Mutex mutex = new Mutex(false, "OCELL_FILE_MUTEX" + fileName);
#if METRO
            var storage = ApplicationData.Current.LocalFolder;
            if (mutex.WaitOne(1000))
            {
                try
                {
                    var file = await storage.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(file, contents);
                }
                catch (Exception)
                {
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
#else
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream File;

            if (mutex.WaitOne(1000))
            {
                try
                {
                    using (File = storage.OpenFile(file, System.IO.FileMode.Create))
                    {
                        File.WriteLine(date.ToString("s"));
                        File.Close();
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
#endif
        }

#if METRO
        public static async Task<string> ReadContentsOfFile(string fileName)
#else
        public static string ReadContentsOfFile(string fileName)
#endif
        {
            Mutex mutex = new Mutex(false, "OCELL_FILE_MUTEX" + fileName);
            string contents = "";
#if METRO
            var storage = ApplicationData.Current.LocalFolder;
            

            if(mutex.WaitOne(1000))
            {
                try
                {
                    var file = await storage.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                    contents = await FileIO.ReadTextAsync(file);
                }
                catch(Exception)
                {
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }

            return contents;
#else
#endif
        }

    }
}