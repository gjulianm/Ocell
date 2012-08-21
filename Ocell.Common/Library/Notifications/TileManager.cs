using System;
using System.Collections.Generic;
using System.Linq;
using TweetSharp;
using Microsoft.Phone.Shell;

namespace Ocell.Library.Notifications
{
    public static class TileManager
    {
        public static void ClearTile()
        {
            StandardTileData tileData = new StandardTileData
            {
                BackContent = "",
                BackTitle = "",
                Count = 0
            };

            ShellTile.ActiveTiles.First().Update(tileData);
        }
    }
}
