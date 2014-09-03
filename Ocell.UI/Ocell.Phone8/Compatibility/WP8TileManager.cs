using Microsoft.Phone.Shell;
using Ocell.Library.Twitter;
using Ocell.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ocell.Compatibility
{
    public class WP8TileManager : TileManager
    {
        public override void ClearMainTileCount()
        {
            IconicTileData data = new IconicTileData()
            {
                Count = 0,
                WideContent1 = "",
                WideContent2 = "",
                WideContent3 = ""
            };

            ShellTile.ActiveTiles.First().Update(data);
        }

        public override void SetNotifications(IEnumerable<TileNotification> notifications)
        {
            if (!notifications.Any())
                return;

            IconicTileData mainTile = new IconicTileData();
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
                    content = Resources.NewMentionsMessages;
                else if (notifications.Any(x => x.Type == TweetType.Mention))
                    content = Resources.NewMentions;
                else if (notifications.Any(x => x.Type == TweetType.Message))
                    content = Resources.NewMessages;
            }

            mainTile.WideContent1 = content;
            mainTile.WideContent2 = String.Format(Resources.ForX, GetChainOfNames(notifications.Select(x => x.To).Distinct().ToList()));
            mainTile.WideContent3 = notifications.First().Message;

            mainTile.Count = notifications.Count();
            ShellTile.ActiveTiles.FirstOrDefault().Update(mainTile);
        }

        public override void SetColumnTweet(string tileString, string content, string author)
        {
            ShellTile Tile = ShellTile.ActiveTiles.FirstOrDefault(item => item.NavigationUri.ToString().Contains(tileString));

            if (Tile != null)
            {
                string line1 = RemoveMention(content), line2 = "";

                if (line1.Length > 33)
                {
                    line2 = line1.Substring(33);
                    line1 = line1.Substring(0, 33);
                }

                IconicTileData TileData = new IconicTileData
                {
                    WideContent1 = author,
                    WideContent2 = line1,
                    WideContent3 = line2
                };

                Tile.Update(TileData);
            }
        }

        private Uri GetNavigationUriFor(TwitterResource resource)
        {
            return new Uri("/MainPage.xaml?column=" + Uri.EscapeDataString(resource.String), UriKind.Relative);
        }


        public override void CreateColumnTile(TwitterResource resource)
        {
#if !BACKGROUND_AGENT
            if (resource == null || ColumnTileIsCreated(resource))
                return;

            IconicTileData columnTile = new IconicTileData
            {
                Title = GetTitle(resource),
                IconImage = new Uri("/Images/ColumnTile.png", UriKind.Relative)
            };

            ShellTile.Create(GetNavigationUriFor(resource), columnTile);
#endif
        }

        public override bool ColumnTileIsCreated(TwitterResource resource)
        {
            return ShellTile.ActiveTiles.Any(x => x.NavigationUri != null && x.NavigationUri.ToString() == GetNavigationUriFor(resource).ToString());
        }

        public override void CreateComposeTile()
        {
#if !BACKGROUND_AGENT
            if (ComposeTileIsCreated())
                return;

            StandardTileData ComposeTile = new StandardTileData
            {
                Title = Localization.Resources.NewTweet,
                BackgroundImage = new Uri("/Images/ComposeTile.png", UriKind.Relative)
            };

            Uri ComposeUri = new Uri("/Pages/NewTweet.xaml");

            ShellTile.Create(ComposeUri, ComposeTile);
#endif
        }

        public bool ComposeTileIsCreated()
        {
            if (ShellTile.ActiveTiles.Count() == 0)
                return false;
            ShellTile ComposeTile = ShellTile.ActiveTiles.FirstOrDefault(item => item != null
                && !string.IsNullOrWhiteSpace(item.NavigationUri.ToString()) &&
                item.NavigationUri.ToString().Contains("NewTweet.xaml"));
            return ComposeTile != null;
        }
    }
}
