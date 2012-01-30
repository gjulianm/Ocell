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

namespace Ocell
{
    public class Account : ITweeter
    {
        public string ProfileImageUrl { get; protected set; }
        public string ScreenName { get; protected set; }
        public string UserToken { get; protected set; }
        public string UserSecret { get; protected set; }
    }
}
