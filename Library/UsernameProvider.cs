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
using TweetSharp;
using System.Linq;

namespace Ocell.Library.Twitter
{
    public class UsernameProvider
    {
        public IList<string> Usernames { get; protected set; }
        public UserToken User { get; set; }
        private TwitterService _service;

        private void GetService()
        {
            if (User == null)
                _service = ServiceDispatcher.GetDefaultService();
            else
                _service = ServiceDispatcher.GetService(User);
        }

        public void Start()
        {
            if (_service == null)
                GetService();

            _service.ListFriends(-1, ReceiveFriends);
        }

        private void ReceiveFriends(TwitterCursorList<TwitterUser> friends, TwitterResponse response)
        {
            if (friends == null || response.StatusCode != HttpStatusCode.OK)
            {
                if (Error != null)
                    Error(this, response);
            }

            if (friends.NextCursor != null && friends.NextCursor != 0 && _service != null)
                _service.ListFriends((long)friends.NextCursor, ReceiveFriends);

            if (Usernames == null)
                Usernames = new List<string>();

            foreach (var user in friends)
                Usernames.Add(user.ScreenName);
        }

        public delegate void OnError(object sender, TwitterResponse response);
        public event OnError Error;
    }
}
