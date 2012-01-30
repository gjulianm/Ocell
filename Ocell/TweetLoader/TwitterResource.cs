using System;

namespace Ocell
{
    public struct TwitterResource
    {
        public ResourceType Type { get; set;}
        public string Data { get; set; }
        public string String
        {
            get
            {
                switch (Type)
                {
                    case ResourceType.Favorites:
                        return "Favorites";
                    case ResourceType.Home:
                        return "Home";
                    case ResourceType.List:
                        return "List_" + Data;
                    case ResourceType.Mentions:
                        return "Mentions";
                    case ResourceType.Messages:
                        return "Messages";
                    case ResourceType.Search:
                        return "Search:" + Data;
                    case ResourceType.Tweets:
                        return "Tweets:" + Data;
                    default:
                        return "Unknown value";
                }
            }
            set
            {
                Data = "";

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
            return (r1.Data == r2.Data && r1.Type == r2.Type);
        }
        public static bool operator!=(TwitterResource r1, TwitterResource r2)
        {
            return (r1.Data != r2.Data || r1.Type != r2.Type);
        }

        public override bool Equals(Object obj)
        {
            // Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;
            TwitterResource p = (TwitterResource)obj;
            return (p.Data == Data && p.Type == Type);
        }
        public override int GetHashCode()
        {
            return Data.GetHashCode() ^ (int)Type;
        }
    };

    public enum ResourceType { Home, Mentions, Messages, List, Search, Favorites, Tweets};
}