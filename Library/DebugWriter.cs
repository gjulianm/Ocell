using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections;
using System.IO.IsolatedStorage;

namespace Ocell.Library
{
    public static class DebugWriter
    {
        private static List<string> _list;

        private static void InitializeIfNull()
        {
            if (_list == null)
            {
                try
                {
                    IsolatedStorageFile Storage = IsolatedStorageFile.GetUserStoreForApplication();
                    IsolatedStorageFileStream File = Storage.OpenFile("Debug", System.IO.FileMode.OpenOrCreate);

                    _list = new List<string>(File.ReadLines());
                    File.Close();
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
            _list.Add(DateTime.Now.ToShortTimeString() + ": " + Line);
        }

        public static void Save()
        {
            try
            {
                UnsafeSave();
            }
            catch (Exception ex )
            {
                ex.ToString();
            }
        }

        private static void UnsafeSave()
        {
            IsolatedStorageFile Storage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream File = Storage.OpenFile("Debug", System.IO.FileMode.Create);
            File.WriteLines(_list);
            File.Close();
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
