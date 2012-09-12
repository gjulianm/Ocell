using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.IO.IsolatedStorage;
using System.IO;

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
            IsolatedStorageFileStream file;

            if (mutex.WaitOne(1000))
            {
                try
                {
                    using (file = storage.OpenFile(fileName, System.IO.FileMode.Create))
                    {
                        using (var writer = new StreamWriter(file))
                        {
                            writer.Write(contents);
                            writer.Close();
                        }
                        file.Close();
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
#if METRO
            var lines = ReadLinesOfFile(fileName);
            return lines.Aggregate("", (accumulate, x) => accumulate + x + '\n');
#else
            Mutex mutex = new Mutex(false, "OCELL_FILE_MUTEX" + fileName);
            var storage = IsolatedStorageFile.GetUserStoreForApplication();
            string contents = "";

            if (mutex.WaitOne(1000))
            {
                try
                {
                    using (var file = storage.OpenFile(fileName, System.IO.FileMode.OpenOrCreate))
                    {
                        using (var reader = new StreamReader(file))
                        {
                            contents = reader.ReadToEnd();
                            reader.Close();
                        }
                        file.Close();
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

            mutex.Dispose();
            return contents;
#endif
        }


        public static void WriteLinesToFile(IEnumerable<string> lines, string fileName)
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
                    FileIO.WriteLinesAsync(file, contents).AsTask().RunSynchronously();
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
            IsolatedStorageFileStream file;

            if (mutex.WaitOne(1000))
            {
                try
                {
                    using (file = storage.OpenFile(fileName, System.IO.FileMode.Create))
                    {
                        using (var writer = new StreamWriter(file))
                        {
                            foreach (var line in lines)
                                writer.WriteLine(line);
                            writer.Close();
                        }
                        file.Close();
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


        public static IEnumerable<string> ReadLinesOfFile(string fileName)
        {
            Mutex mutex = new Mutex(false, "OCELL_FILE_MUTEX" + fileName);
            List<string> lines = new List<string>();
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
                    lines = readTask.Result;
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
#else
            var storage = IsolatedStorageFile.GetUserStoreForApplication();
            
            if (mutex.WaitOne(1000))
            {
                try
                {
                    using (var file = storage.OpenFile(fileName, System.IO.FileMode.OpenOrCreate))
                    {
                        using (var reader = new StreamReader(file))
                        {
                            while (!reader.EndOfStream)
                                lines.Add(reader.ReadLine());
                            reader.Close();
                        }
                        file.Close();
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
            mutex.Dispose();
            return lines;
        }

        /// <summary>
        /// Read a series of blocks from a file, return them in a list. Block = string with whatever char in them, including newlines.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>List of blocks.</returns>
        public static IEnumerable<string> ReadBlocksOfFile(string fileName)
        {
            char separator = char.MaxValue;
            string contents = ReadContentsOfFile(fileName);
            return contents.Split(separator);
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