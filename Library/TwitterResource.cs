using System;

namespace Ocell.Library
{
    public struct TwitterResource
    {        
        public ResourceType Type { get; set;}
        public UserToken User { get; set;}
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
        public string String
        {
            get
            {
                string value = User.ScreenName + ";";
                switch (Type)
                {
                    case ResourceType.Favorites:
                        value+="Favorites";
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
                        value+= "Search:" + Data;
                        break;
                    case ResourceType.Tweets:
                        value += "Tweets:" + Data;
                        break;
                    default:
                        value += "Unknown value";
                        break;
                }
                return value;
            }
            set
            {
                Data = "";
                
                if (User == null)
                    User = new UserToken();

                int SemiColonIndex = value.IndexOf(';');
                User.ScreenName = value.Substring(0, SemiColonIndex);
                value = value.Substring(SemiColonIndex + 1);
                if (!value.Contains(":"))
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
                    Data = value.Substring(value.IndexOf(':') + 1);
                }
                else if (value.Contains("Search:"))
                {
                    Type = ResourceType.Search;
                    Data = value.Substring(value.IndexOf(':') + 1);
                }
                else if (value.Contains("Tweets:"))
                {
                    Type = ResourceType.Tweets;
                    Data = value.Substring(value.IndexOf(':') + 1);
                }
                else
                    Type = ResourceType.Home;
            }
        }

        public static bool operator==(TwitterResource r1, TwitterResource r2)
        {
            return (r1.Data == r2.Data && r1.Type == r2.Type && r1.User == r2.User);
        }
        public static bool operator!=(TwitterResource r1, TwitterResource r2)
        {
            return (r1.Data != r2.Data || r1.Type != r2.Type || r1.User != r2.User);
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

    public enum ResourceType { Home, Mentions, Messages, List, Search, Favorites, Tweets};
}