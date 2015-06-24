﻿using System.Diagnostics;
using Ocell.Library.Notifications;

namespace Ocell.Library.Twitter
{
    [DebuggerDisplay("{ScreenName}")]
    public class UserToken
    {
        public string Key { get; set; }
        public string Secret { get; set; }
        public string ScreenName { get; set; }
        public long? Id { get; set; }
        public string AvatarUrl { get; set; }
        public NotificationPreferences Preferences;


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

        public override string ToString()
        {
            return ScreenName;
        }
    }
}
