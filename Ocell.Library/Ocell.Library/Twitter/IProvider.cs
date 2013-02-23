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
namespace Ocell.Library.Twitter
{
    public delegate void OnError(object sender, TwitterResponse response);

    public interface IUserProvider
    {
        SafeObservable<TwitterUser> Users { get; set; }
        UserToken User { get; set; }
        bool GetFollowers { get; set; }
        bool GetFollowing { get; set; }

        void Start();

        event OnError Error;
        event EventHandler Finished;
    }
}
