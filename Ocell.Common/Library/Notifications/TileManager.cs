using System;
using System.Collections.Generic;
using System.Linq;
using TweetSharp;
#if WINDOWS_PHONE
using Microsoft.Phone.Shell;
#else
#endif

namespace Ocell.Library.Notifications
{
    public static class TileManager
    {
        public static void ClearTile()
        {
#if WINDOWS_PHONE
            StandardTileData tileData = new StandardTileData
            {
                BackContent = "",
                BackTitle = "",
                Count = 0
            };

            ShellTile.ActiveTiles.First().Update(tileData);
#else
            //TODO: Clear W8 tile here.
#endif
        }
    }
}
