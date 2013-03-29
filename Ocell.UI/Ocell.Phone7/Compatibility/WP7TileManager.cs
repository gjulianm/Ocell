using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocell.Compatibility
{
    public class WP7TileManager :ITileManager
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
    }
}
