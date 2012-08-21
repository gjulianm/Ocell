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
    public class UserTokenFiller
    {
        public UserToken Token { get; set; }

        public UserTokenFiller(UserToken token)
        {
            Token = token;
        }

        public void FillUserData()
        {
            if (!(string.IsNullOrWhiteSpace(Token.ScreenName) || string.IsNullOrWhiteSpace(Token.AvatarUrl) || Token.Id == null))
                return;

            ITwitterService srv = ServiceDispatcher.GetService(Token);

            srv.GetUserProfile(ReceiveUserProfile);
        }

        protected void ReceiveUserProfile(TwitterUser user, TwitterResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
                return;

            Token.ScreenName = user.ScreenName;
            Token.Id = user.Id;
            Token.AvatarUrl = user.ProfileImageUrl;

            if (UserDataFilled != null)
                UserDataFilled(Token);
        }

        public delegate void OnUserDataFilled(UserToken Token);
        public event OnUserDataFilled UserDataFilled;
    }
}
