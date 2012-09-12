using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Linq;

namespace Ocell.Library
{
    public static class Logger
    {
        private static List<string> _list;

        private static void InitializeIfNull()
        {
            if (_list == null)
            {
                try
                {
                    _list = FileAbstractor.ReadLinesOfFile("DEBUG").ToList();
                }
                catch (Exception)
                {
                    _list = new List<string>();
                }
            }
        }

        [Conditional("DEBUG")]
        public static void Add(string Line)
        {
            InitializeIfNull();
            _list.Add(Line);
        }

        [Conditional("DEBUG")]
        public static void Log(string message)
        {
            string msg = string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss.ff"), message);
            Debug.WriteLine(msg);
            Add(msg);
        }


        public static void Save()
        {
            try
            {
                UnsafeSave();
            }
            catch (Exception)
            {
            }
        }

        private static void UnsafeSave()
        {
            FileAbstractor.WriteLinesToFile(_list, "DEBUG");
        }

        public static void Clear()
        {
            InitializeIfNull();
            _list.Clear();
        }

        public static IEnumerable<string> ReadAll()
        {
            InitializeIfNull();
            return _list;
        }
    }
}
