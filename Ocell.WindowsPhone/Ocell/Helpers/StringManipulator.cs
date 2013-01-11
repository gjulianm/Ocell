using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
namespace Ocell
{
    public static class StringManipulator
    {
        public static IEnumerable<string> GetUserNames(string text)
        {
            // I was too lazy to build the Regex myself, so Google is my friend. Source: http://shahmirj.com/blog/extracting-twitter-usertags-using-regex.
            var regex = new Regex("(?<=^|(?<=[^a-zA-Z0-9-_\\.]))@([A-Za-z]+[A-Za-z0-9_]+)");
            
            foreach(Match match in regex.Matches(text))
                yield return match.Value;
        }

        public static string RemoveHtmlTags(string text) 
        {
            bool onTag = false;
            string result = "";
            int i;

            for (i = 0; i < text.Length; i++)
            {
                if (onTag && text[i] == '>')
                    onTag = false;
                else if (!onTag)
                {
                    if (text[i] == '<')
                        onTag = true;
                    else
                        result += text[i];
                }
            }

            return result;
        }
    }
}
