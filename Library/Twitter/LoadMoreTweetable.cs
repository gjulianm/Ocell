using System;
using TweetSharp;

namespace Ocell.Library.Twitter
{
    public class LoadMoreTweetable : ITweetable
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public string TextAsHtml { get; set; }
        public ITweeter Author { get { return null; } }
        public DateTime CreatedDate { get; set; }
        public TwitterEntities Entities { get { return null; } }
        public string RawSource { get; set; }
        public string AuthorName { get; set; }
        public string CleanText { get; set; }
    }
}
