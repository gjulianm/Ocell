using AncoraMVVM.Base;
using System;
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
