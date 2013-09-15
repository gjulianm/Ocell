using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace Ocell.Library
{
    public class MediaLinkParser
    {
        private Dictionary<string, string> matchDic = new Dictionary<string, string> {
			{@"http[s]?://instagram.com/p/([-\w]+)/?", "http://instagr.am/p/__1__/media/?size=l"},
			{@"http[s]?://twitpic.com/(\w+)/?" , "http://twitpic.com/show/thumb/__1__"},
			{@"http[s]?://imgur.com/(\w+)/?" , "http://i.imgur.com/__1__l.png"},
			{@"(http[s]?://.*\.(png|jpg|gif))" , "__1__"},
			{@"http[s]?://9gag\.com/gag/([0-9\w]+)/?" , "http://d24w6bsrhbeh9d.cloudfront.net/photo/__1___700b.jpg"},
			{@"http[s]?://yfrog.com/(\w+)/?" , "http://yfrog.com/__1__:medium"}
		};

        public string GetMediaUrl(string url)
        {
            foreach (var pair in matchDic)
            {
                var match = Regex.Match(url, pair.Key);

                if (match.Success)
                    return Regex.Replace(pair.Value, "__1__", match.Groups[1].Value);
            }

            return null;
        }

        public bool TryGetMediaUrl(string url, out string mediaUrl)
        {
            mediaUrl = GetMediaUrl(url);

            return mediaUrl != null;
        }
    }
}
