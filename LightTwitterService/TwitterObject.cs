using System;
using System.Text.RegularExpressions;

namespace Ocell.LightTwitterService
{
    public class SerializationException : Exception
    {
        public SerializationException(string message)
            : base(message)
        {
        }
    }

    public class TwitterObject
    {
        string _contents;

        public TwitterObject(string contents)
        {
            _contents = contents;
        }

        public string GetProperty(string propertyName)
        {
            Regex findProperty = new Regex("\"" + propertyName + "\" *:");

            var matches = findProperty.Matches(_contents);
            int startContent;
            string whereToSearch = _contents;

            if (matches.Count == 0)
                throw new SerializationException("Property " + propertyName + " not found.");
            else if (matches.Count == 1)
                startContent = matches[0].Index + matches[0].Length;
            else
            {
                Regex removeBrackets = new Regex("\": *{.*?},");
                string aux = removeBrackets.Replace(_contents, "\": \"\",");
                matches = findProperty.Matches(aux);

                if (matches.Count == 0)
                    return new TwitterObject(findProperty.Match(_contents).Groups[0].Value).GetProperty(propertyName);
                else
                {
                    whereToSearch = aux;
                    startContent = matches[0].Index + matches[0].Length;
                }
            }

            while (startContent < whereToSearch.Length && whereToSearch[startContent] == ' ')
                startContent++;

            if (startContent == whereToSearch.Length)
                throw new SerializationException("Malformed contents.");

            int endContent = startContent;
            char finalDelimiter;

            if (whereToSearch[startContent] == '"')
            {
                startContent++;
                finalDelimiter = '"';
            }
            else if (whereToSearch[startContent] == '{')
            {
                startContent++;
                finalDelimiter = '}';
            }
            else if (whereToSearch[startContent] == '[')
            {
                startContent++;
                finalDelimiter = ']';
            }
            else
                finalDelimiter = ',';

            do
            {
                endContent = whereToSearch.IndexOf(finalDelimiter, endContent + 1);
            }
            while (endContent + 1 < whereToSearch.Length && endContent != -1 && whereToSearch[endContent - 1] == '\\');

            if (endContent == -1)
            {
                if (whereToSearch[whereToSearch.Length - 1] == '}')
                    return whereToSearch.Remove(whereToSearch.Length - 1).Substring(startContent);
                else
                    return whereToSearch.Substring(startContent);
            }
            else
                return whereToSearch.Substring(startContent, endContent - startContent);
        }

        public bool TryGetProperty(string propertyName, out string value)
        {
            try
            {
                value = GetProperty(propertyName);
                return true;
            }
            catch (Exception)
            {
                value = "";
                return false;
            }
        }

        public override string ToString()
        {
            return _contents;
        }
    }
}