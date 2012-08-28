using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ocell.Library
{
    public enum LogType { Message, Warning, Error }

    public static class DebugWriter
    {
        private static List<string> _list;

        private static void InitializeIfNull()
        {
            if (_list == null)
            {
                try
                {
                    _list = FileAbstractor.ReadLinesOfFile("Debug").ToList();
                }
                catch (Exception)
                {
                    _list = new List<string>();
                }
            }
        }

        public static void Add(string Line)
        {
            InitializeIfNull();
            Debug.WriteLine(Line);
            _list.Add(Line);
        }

        public static void Log(string message, LogType type = LogType.Message)
        {
            string msgType = GetString(type);

            Add(String.Format("{0} - {1}: {2}", DateTime.Now.ToString("G"), message));
        }

        private static string GetString(LogType type)
        {
            switch (type)
            {
                case LogType.Message:
                    return "Message";
                case LogType.Error:
                    return "Error";
                case LogType.Warning:
                    return "Warning";
                default:
                    return "Unknown";
            }
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
            FileAbstractor.WriteLinesToFile(_list, "Debug");
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
