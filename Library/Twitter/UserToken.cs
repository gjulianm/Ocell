using System.Net;
using System.Windows.Input;
using Ocell.Library.Notifications;
using TweetSharp;

namespace Ocell.Library.Twitter
{
    public class UserToken
    {
        public string Key {get; set;}
        public string Secret { get; set; }
        public string ScreenName { get; set; }
        public int? Id { get; set; }
        public string AvatarUrl { get; set; }
        public NotificationPreferences Preferences;
        
        public void FillUserData()
        {
            if (!(string.IsNullOrWhiteSpace(ScreenName) || string.IsNullOrWhiteSpace(AvatarUrl) || Id == null))
                return;

            ITwitterService srv = ServiceDispatcher.GetService(this);
            srv.GetUserProfile(ReceiveUserProfile);
        }

        protected void ReceiveUserProfile(TwitterUser user, TwitterResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
                return;

            ScreenName = user.ScreenName;
            Id = user.Id;
            AvatarUrl = user.ProfileImageUrl;

            if (UserDataFilled != null)
                UserDataFilled(this);
        }

        public delegate void OnUserDataFilled(UserToken Token);
        public event OnUserDataFilled UserDataFilled;

        public static bool operator ==(UserToken a, UserToken b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Key == b.Key && a.Secret == b.Secret && a.ScreenName == b.ScreenName;
        }

        public static bool operator !=(UserToken a, UserToken b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if ((obj as UserToken) == null)
                return false;
            UserToken a = obj as UserToken;
            return a.Key == Key && a.Secret == Secret && a.ScreenName == ScreenName;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
