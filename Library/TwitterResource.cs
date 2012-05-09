using System;

namespace Ocell.Library.Twitter
{
    public struct TwitterResource
    {        
        public ResourceType Type { get; set;}
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
        public string String
        {
            get
            {
                string value; 
                if(User != null)
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
                        value+= "Search:" + Data;
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

    public enum ResourceType { Home, Mentions, Messages, List, Search, Favorites, Tweets, Conversation};
}