using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocell.Compatibility
{
    public class WP8TileManager : ITileManager
    {
        public void ClearMainTileCount()
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
    }
}
