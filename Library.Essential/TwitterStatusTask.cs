using System;
using System.Collections.Generic;
using System.Net;
using Ocell.Library.Twitter;
using TweetSharp;
namespace Ocell.Library.Tasks
{
    public class TwitterStatusTask
    {
        public string Text { get; set; }
        public long InReplyTo { get; set; }
        public IList<UserToken> Accounts { get; set; }
        public DateTime Scheduled { get; set; }
    }
}
