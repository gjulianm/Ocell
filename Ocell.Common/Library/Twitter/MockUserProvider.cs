using System;
using TweetSharp;

namespace Ocell.Library.Twitter
{
    public class MockUserProvider : IUserProvider
    {
        public SafeObservable<TwitterUser> Users { get; set; }
        public UserToken User { get; set; }
        public bool GetFollowers { get; set; }
        public bool GetFollowing { get; set; }

        public MockUserProvider()
        {
            Users = new SafeObservable<TwitterUser>();
        }

        public void Start()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            for(int i = 0; i<50; i++)
                Users.Add(new TwitterUser { ScreenName = rand.Next(1253508).ToString() });
        }

        public void RaiseFinished()
        {
            if(Finished != null)
                Finished(this, new EventArgs());
        }

        public event OnError Error;
        public event EventHandler Finished;
    }
}
