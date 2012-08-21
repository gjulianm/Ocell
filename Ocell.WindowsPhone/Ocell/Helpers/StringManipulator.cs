using System.Collections.Generic;

namespace Ocell
{
    public static class StringManipulator
    {
        public static IEnumerable<string> GetUserNames(string text)
        {
            int i;
            string user = "";
            bool onUser = false;
            List<string> list = new List<string>();

            for (i = 0; i < text.Length; i++)
            {
                if (!onUser)
                    onUser = (text[i] == '@');
                else if (text[i] == ' ')
                {
                    onUser = false;
                    yield return user;
                    user = "";
                }
                else
                    user += text[i];
            }
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
