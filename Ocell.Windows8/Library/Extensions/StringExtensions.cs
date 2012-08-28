using System.Linq;

namespace Ocell.Library
{
    public static class StringExtensions
    {
        public static bool Contains(this string str, char character)
        {
            return str.ToCharArray().Contains(character);
        }
    }
}
