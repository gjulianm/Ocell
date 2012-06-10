using System;
using System.Collections.Generic;
using System.Linq;
using TweetSharp;
using Microsoft.Phone.Shell;

namespace Ocell.Library.Notifications
{
    public static class TileManager
    {
        public static void UpdateTile(IEnumerable<TwitterStatus> Statuses, IEnumerable<TwitterDirectMessage> Messages)
        {
            string StatusesStr, MessagesStr;
            int count = 0;

            if (Statuses != null)
                count += Statuses.Count();
            if (Messages != null)
                count += Messages.Count();

            if (Statuses == null || Statuses.Count() == 0)
                StatusesStr = "";
            else if (Statuses.Count() == 1)
                StatusesStr = Statuses.First().Author.ScreenName + " mentioned you";
            else
                StatusesStr = Statuses.Count().ToString() + " new mentions";

            if (Messages == null || Messages.Count() == 0)
                MessagesStr = "";
            else if (Messages.Count() == 1)
                MessagesStr = Messages.First().Author.ScreenName + " mentioned you";
            else
                MessagesStr = Messages.Count().ToString() + " new mentions";

            if (StatusesStr.Length > 0 && MessagesStr.Length > 0)
                StatusesStr += Environment.NewLine;

            StandardTileData TileData = new StandardTileData
            {
                BackContent = StatusesStr + MessagesStr,
                Count = count,
            };

            ShellTile.ActiveTiles.First().Update(TileData);
        }
    }
}
