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
using TweetSharp;

namespace Ocell.Library
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
    }
}
