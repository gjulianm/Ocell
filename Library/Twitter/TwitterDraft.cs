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
