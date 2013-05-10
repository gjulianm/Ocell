using Microsoft.Phone.Shell;
using Ocell.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocell.Compatibility
{
    public class WP7TileManager : TileManager
    {
        public void ClearMainTileCount()
        {
            StandardTileData tileData = new StandardTileData
            {
                BackContent = "",
                BackTitle = "",
                Count = 0
            };

            ShellTile.ActiveTiles.First().Update(tileData);
        }



        public void SetNotifications(IEnumerable<TileNotification> notifications)
        {
            if (!notifications.Any())
                return;

            StandardTileData mainTile = new StandardTileData();
            string content = "";

            if (notifications.Count() == 1)
            {
                var not = notifications.First();
                if (not.Type == TweetType.Mention)
                    content = String.Format(Resources.NewMention, not.From);
                else
                    content = String.Format(Resources.NewMessage, not.From);
            }
            else
            {
                if (notifications.Any(x => x.Type == TweetType.Mention) && notifications.Any(x => x.Type == TweetType.Message))
                    content = String.Format(Resources.NewXMentionsMessages, notifications.Count());
                else if (notifications.Any(x => x.Type == TweetType.Mention))
                    content = String.Format(Resources.NewXMentions, notifications.Count());
                else if (notifications.Any(x => x.Type == TweetType.Message))
                    content = String.Format(Resources.NewXMessages, notifications.Count());
            }

            mainTile.BackTitle = String.Format(Resources.ForX, GetChainOfNames(notifications.Select(x => x.To).Distinct().ToList()));
            mainTile.BackContent = content;

            mainTile.Count = notifications.Count();
            ShellTile.ActiveTiles.FirstOrDefault().Update(mainTile);
        }

        public void SetColumnTweet(string tileString, string content, string author)
        {
            ShellTile Tile = ShellTile.ActiveTiles.FirstOrDefault(item => item.NavigationUri.ToString().Contains(tileString));

            if (Tile != null)
            {
                StandardTileData TileData = new StandardTileData
                {
                    BackContent = RemoveMention(content),
                    BackTitle = author
                };
                Tile.Update(TileData);
            }
        }
    }
}
