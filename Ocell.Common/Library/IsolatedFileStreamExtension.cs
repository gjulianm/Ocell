using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
#if !BACKGROUND_AGENT
using System.Linq;
#endif

namespace Ocell.Library
{
    public static class IsolatedFileStreamExtension
    {		
        // YOU IDIOT!!!! Returning on yield-return makes it read again and this causes errors! Just return List<T> 
        //      (although you can continue returning it as IEnumerable<string>).
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
                Strings.Add(line);
            }

            return Strings;
        }
		
	    public static string ReadLine(this IsolatedStorageFileStream File)
	    {
		    IEnumerable<string> Lines = File.ReadLines().ToList();
		    if(Lines != null && Lines.Count() != 0)
			    return Lines.FirstOrDefault();
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