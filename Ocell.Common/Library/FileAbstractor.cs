using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

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
                    DebugWriter.Log("Exception writing to file " + fileName, LogType.Warning);
                    throw;
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
                    throw;
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
#if METRO
            var lines = await ReadLinesOfFile(fileName);
#else
            var lines = ReadLinesOfFile(fileName);
#endif
            return lines.Aggregate("", (accumulate, x) => accumulate + x + '\n');
        }


        public static void WriteLinesToFile(IEnumerable<string> lines, string fileName)
        {
            var contents = lines.Aggregate("", (accumulate, x) => accumulate + x + '\n');
            WriteContentsToFile(contents, fileName);
        }

#if METRO
        public static async Task<IEnumerable<string>> ReadLinesOfFile(string fileName)
#else
        public static string IEnumerable<string> ReadLinesOfFile(string fileName)
#endif
        {
            Mutex mutex = new Mutex(false, "OCELL_FILE_MUTEX" + fileName);
            IEnumerable<string> contents = new List<string>();
#if METRO
            var storage = ApplicationData.Current.LocalFolder;


            if (mutex.WaitOne(1000))
            {
                try
                {
                    var file = await storage.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                    contents = await FileIO.ReadLinesAsync(file);
                }
                catch (Exception)
                {
                    throw;
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