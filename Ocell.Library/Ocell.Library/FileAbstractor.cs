using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.IO.IsolatedStorage;
using System.IO;
using Ocell.Library.Security;

namespace Ocell.Library
{
    public static class FileAbstractor
    {
        public static void WriteContentsToFile(string contents, string fileName)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream file;

            MutexUtil.DoWork("OCELL_FILE_MUTEX" + fileName, () =>
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
            });
        }

        public static string ReadContentsOfFile(string fileName)
        {
            var storage = IsolatedStorageFile.GetUserStoreForApplication();
            string contents = "";

            MutexUtil.DoWork("OCELL_FILE_MUTEX" + fileName, () =>
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
            });

            return contents;
        }


        public static void WriteLinesToFile(IEnumerable<string> lines, string fileName)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream file;

            MutexUtil.DoWork("OCELL_FILE_MUTEX" + fileName, () =>
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
            });
        }


        public static IEnumerable<string> ReadLinesOfFile(string fileName)
        {

            List<string> lines = new List<string>();

            var storage = IsolatedStorageFile.GetUserStoreForApplication();

            MutexUtil.DoWork("OCELL_FILE_MUTEX" + fileName, () =>
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
            });

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

        public static Stream GetFileStream(string fileName)
        {
            var storage = IsolatedStorageFile.GetUserStoreForApplication();
            return storage.OpenFile(fileName, System.IO.FileMode.OpenOrCreate);
        }
    }
}