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


        public static void WriteContentsToFile(string contents, string fileName)
        {
            Mutex mutex = new Mutex(false, "OCELL_FILE_MUTEX" + fileName);
#if METRO
            var storage = ApplicationData.Current.LocalFolder;
            if (mutex.WaitOne(1000))
            {
                try
                {
                    var fileTask = storage.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting).AsTask();
                    fileTask.Wait();
                    var file = fileTask.Result;
                    FileIO.WriteTextAsync(file, contents).AsTask().RunSynchronously();
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


        public static string ReadContentsOfFile(string fileName)
        {
            var lines = ReadLinesOfFile(fileName);
            return lines.Aggregate("", (accumulate, x) => accumulate + x + '\n');
        }


        public static void WriteLinesToFile(IEnumerable<string> lines, string fileName)
        {
            var contents = lines.Aggregate("", (accumulate, x) => accumulate + x + '\n');
            WriteContentsToFile(contents, fileName);
        }


        public static IEnumerable<string> ReadLinesOfFile(string fileName)
        {
            Mutex mutex = new Mutex(false, "OCELL_FILE_MUTEX" + fileName);
            IEnumerable<string> contents = new List<string>();
#if METRO
            var storage = ApplicationData.Current.LocalFolder;


            if (mutex.WaitOne(1000))
            {
                try
                {
                    var fileTask = storage.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists).AsTask();
                    fileTask.Wait();
                    var file = fileTask.Result;
                    var readTask = FileIO.ReadLinesAsync(file).AsTask();
                    readTask.Wait();
                    contents = readTask.Result;
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

        /// <summary>
        /// Read a series of blocks from a file, return them in a list. Block = string with whatever char in them, including newlines.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>List of blocks.</returns>
        public static IEnumerable<string> ReadBlocksOfFile(string fileName)
        {
            char separator = char.MaxValue;
            List<string> Strings = new List<string>();

            string contents = ReadContentsOfFile(fileName);
            string line = "";
            int newlineIndex;

            while ((newlineIndex = contents.IndexOf(separator)) != -1)
            {
                line = contents.Substring(0, newlineIndex);
                contents = contents.Substring(newlineIndex + 1);
                Strings.Add(line);
            }

            return Strings;
        }

        /// <summary>
        /// Writes a series of blocks to a file. A block can contain any character (including newlines),
        /// and it will still be returned without problems.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="blocks"></param>
        public static void WriteBlocksToFile(IEnumerable<string> blocks, string fileName)
        {
            char separator = char.MaxValue;

            string contents = "";

            foreach (var str in blocks)
                contents += str + separator;

            WriteContentsToFile(contents, fileName);
        }

    }
}