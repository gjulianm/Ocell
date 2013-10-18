using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;
using DanielVaughan.ComponentModel;
using DanielVaughan.Windows;
using Microsoft.Phone.Tasks;
using Ocell.Library;
using Ocell.Library.Twitter;
using TweetSharp;

namespace Ocell.Pages.Elements
{
    public class DMConversationModel : ExtendedViewModelBase
    {
        string pairName;
        public string PairName
        {
            get { return pairName; }
            set { Assign("PairName", ref pairName, value); }
        }

        public DMConversationModel()
            : base("DMConversation")
        {
        }
    }
}
