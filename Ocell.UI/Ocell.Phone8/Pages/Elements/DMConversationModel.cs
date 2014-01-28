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
using PropertyChanged;

namespace Ocell.Pages.Elements
{
    [ImplementPropertyChanged]
    public class DMConversationModel : ExtendedViewModelBase
    {
        public string PairName { get; set; }

        public DMConversationModel()
            : base("DMConversation")
        {
        }
    }
}
