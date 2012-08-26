using System;
using System.Collections.Generic;

namespace Ocell.Library.Twitter
{
    public class TwitterDraft
    {
        public string Text { get; set; }
        public long? ReplyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Scheduled { get; set; }
        public List<UserToken> Accounts { get; set; }
    }
}
