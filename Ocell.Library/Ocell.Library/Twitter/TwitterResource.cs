using System;
using Ocell.Localization;
using System.Diagnostics;

namespace Ocell.Library.Twitter
{
    [DebuggerDisplay("{String}")]
    public class TwitterResource
    {
        public ResourceType Type { get; set; }
        public UserToken User { get; set; }
        private string _data;
        public string Data
        {
            get
            {
                if (_data == null)
                    return "";
                else
                    return _data;
            }
            set
            {
                _data = value;
            }
        }

        public string Title
        {
            get
            {
                string title;
                switch (Type)
                {
                    case ResourceType.Favorites:
                        title = Resources.Favorites;
                        break;
                    case ResourceType.Home:
                        title = Resources.Home;
                        break;
                    case ResourceType.List:
                        title = Data;
                        break;
                    case ResourceType.Mentions:
                        title = Resources.Mentions;
                        break;
                    case ResourceType.Messages:
                        title = Resources.Messages;
                        break;
                    case ResourceType.Search:
                        title = Data;
                        break;
                    case ResourceType.Tweets:
                        title = Data;
                        break;
                    case ResourceType.Conversation:
                        title = Resources.Conversation;
                        break;
                    default:
                        title = Resources.UnknownValue;
                        break;
                }

                title = title.ToLowerInvariant();
                int posSemicolon = title.IndexOf(';');
                int posPoints = title.IndexOf(':');
                int posSlash = title.IndexOf('/');

                int whereToCut = Math.Max(Math.Max(posPoints, posSlash), posSemicolon) + 1;

                return title.Substring(whereToCut);
            }
        }

        public string String
        {
            get
            {
                string value;
                if (User != null)
                    value = User.ScreenName + ";";
                else
                    value = ";";
                switch (Type)
                {
                    case ResourceType.Favorites:
                        value += "Favorites";
                        break;
                    case ResourceType.Home:
                        value += "Home";
                        break;
                    case ResourceType.List:
                        value += "List:" + Data;
                        break;
                    case ResourceType.Mentions:
                        value += "Mentions";
                        break;
                    case ResourceType.Messages:
                        value += "Messages";
                        break;
                    case ResourceType.Search:
                        value += "Search:" + Data;
                        break;
                    case ResourceType.Tweets:
                        value += "Tweets:" + Data;
                        break;
                    case ResourceType.Conversation:
                        value += "Conversation:" + Data;
                        break;
                    default:
                        value += "Unknown value";
                        break;
                }
                return value;
            }
            set
            {
                try
                {
                    UnsafeSetString(value);
                }
                catch (Exception)
                {
                }
            }
        }

        private void UnsafeSetString(string value)
        {
            Data = "";

            if (User == null)
                User = new UserToken();

            int SemiColonIndex = value.IndexOf(';');
            if (SemiColonIndex != -1)
                User.ScreenName = value.Substring(0, SemiColonIndex);

            if ((SemiColonIndex + 1) <= value.Length)
                value = value.Substring(SemiColonIndex + 1);

            int ColonIndex = value.IndexOf(':');
            ColonIndex = Math.Min(ColonIndex + 1, value.Length);

            if (ColonIndex == 0)
            {
                if (value == "Favorites")
                    Type = ResourceType.Favorites;
                else if (value == "Home")
                    Type = ResourceType.Home;
                else if (value == "Mentions")
                    Type = ResourceType.Mentions;
                else if (value == "Messages")
                    Type = ResourceType.Messages;
            }
            else if (value.Contains("List:"))
            {
                Type = ResourceType.List;
                Data = value.Substring(ColonIndex);
            }
            else if (value.Contains("Search:"))
            {
                Type = ResourceType.Search;
                Data = value.Substring(ColonIndex);
            }
            else if (value.Contains("Tweets:"))
            {
                Type = ResourceType.Tweets;
                Data = value.Substring(ColonIndex);
            }
            else if (value.Contains("Conversation:"))
            {
                Type = ResourceType.Conversation;
                Data = value.Substring(ColonIndex);
            }
            else
                Type = ResourceType.Home;
        }

        public static bool operator ==(TwitterResource r1, TwitterResource r2)
        {
            if (System.Object.ReferenceEquals(r1, r2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)r2 == null) || ((object)r1 == null))
            {
                return false;
            }

            return (r1.Data == r2.Data && r1.Type == r2.Type && r1.User == r2.User);
        }
        public static bool operator !=(TwitterResource r1, TwitterResource r2)
        {
            return !(r1 == r2);
        }

        public override bool Equals(Object obj)
        {
            // Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;
            TwitterResource p = (TwitterResource)obj;
            return (p.Data == Data && p.Type == Type && p.User == User);
        }
        public override int GetHashCode()
        {
            return (int)Type ^ Data.GetHashCode() ^ User.Id.GetHashCode();
        }
    };

    public enum ResourceType { Home, Mentions, Messages, List, Search, Favorites, Tweets, Conversation };
}