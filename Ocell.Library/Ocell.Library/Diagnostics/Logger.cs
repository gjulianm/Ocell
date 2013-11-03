using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ocell.Library
{
    [Flags]
    public enum LogLevel { Message, Warning, Error, Fatal };

    public static class Logger
    {
        private static List<string> Lines = new List<string>();

        public static void Trace(string message, LogLevel level = LogLevel.Message,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0)
        {
            var lastSlash = sourceFilePath.LastIndexOf("\\") + 1;
            sourceFilePath = sourceFilePath.Substring(lastSlash, sourceFilePath.Length - lastSlash);
            message = String.Format("{0} [{1}]: {2}" + Environment.NewLine + "\t==WHERE== {3} at {4}:{5}", DateTime.Now, level, message, memberName, sourceFilePath, sourceLineNumber);
            Lines.Add(message);
            Debug.WriteLine(message);
        }

        public static IEnumerable<string> LogHistory { get { return Lines; } }
        public static string LogWithoutMessages
        {
            get
            {
                var msgString = String.Format("[{0}]", LogLevel.Message);
                var logs = LogHistory.Where(x => !x.Contains(msgString));

                StringBuilder builder = new StringBuilder();
                foreach (var str in logs)
                    builder.AppendLine(str);

                return builder.ToString();
            }
        }

        public static string LogAsString
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach (var str in Lines)
                    builder.AppendLine(str);

                return builder.ToString();
            }
        }
    }
}