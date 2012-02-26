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
using System.IO.IsolatedStorage;
using System.Text;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ocell.Library
{
	public static class IsolatedFileStreamExtension
	{		
		public static IEnumerable<string> ReadLines(this IsolatedStorageFileStream File)
		{
            char separator = char.MaxValue;
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            List<string> Strings = new List<string>();

            string contents;
            byte[] bytes= new byte[File.Length];
            string line = "";
            int NewlineIndex;

            File.Read(bytes, 0, (int)File.Length);
            contents = new string(encoding.GetChars(bytes));

            while ((NewlineIndex = contents.IndexOf(separator)) != -1)
            {
                line = contents.Substring(0, NewlineIndex);
                contents = contents.Substring(NewlineIndex + 1);
                yield return line;
            }
        }
		
		public static string ReadLine(this IsolatedStorageFileStream File)
		{
			IEnumerable<string> Lines = File.ReadLines();
			if(Lines != null && Lines.Count() != 0)
				return Lines.First();
            return "";
		}
		
		public static void WriteLines(this IsolatedStorageFileStream File, IEnumerable<string> Lines)
		{
			char separator = char.MaxValue;
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            string contents = "";
            byte[] bytes;

            foreach (var str in Lines)
                contents += str + separator;

            bytes = encoding.GetBytes(contents);
            File.Write(bytes, 0, bytes.Length);
    	}
    	
    	public static void WriteLine(this IsolatedStorageFileStream File, string Line)
    	{
    		List<string> Lines = new List<string>();
    		Lines.Add(Line);
    		File.WriteLines(Lines);
    	}
    }
}