using Ocell.Library.Twitter;
using Ocell.Localization;
using System.Collections.Generic;
using System.Linq;

namespace Ocell.Compatibility
{
    public enum TweetType { Mention, Message }

    public struct TileNotification
    {
        public TweetType Type { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
    }

    public abstract class TileManager
    {
        public abstract void ClearMainTileCount();
        public abstract void SetNotifications(IEnumerable<TileNotification> notifications);
        public abstract void SetColumnTweet(string tileString, string content, string author);
        public abstract void CreateColumnTile(TwitterResource resource);
        public abstract void CreateComposeTile(); // Avoid Store complaints about "mehhh can't use ShellTile.Create in backgroundAgent"
        public abstract bool ColumnTileIsCreated(TwitterResource resource);

        protected string GetChainOfNames(List<string> names)
        {
            string content = "";
            if (names == null || !names.Any())
                return content;

            int i = 0;
            content += names[i];
            i++;

            for (; i < names.Count - 1; i++)
                content += ", " + names[i];

            if (i == names.Count - 1)
                content += " " + Resources.And + " " + names[i];

            return content;
        }

        protected string RemoveMention(string Tweet)
        {
            if (Tweet[0] == '@')
                Tweet = (char)8203 + Tweet;
            return Tweet;
        }

        protected string GetTitle(TwitterResource Resource)
        {
            string title = Resource.Title.ToLowerInvariant();

            title = char.ToUpper(title[0]) + title.Substring(1);

            return title;
        }
    }
}
