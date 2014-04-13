using Microsoft.Phone.Shell;
using Ocell.Library.Twitter;
using System;
using System.Linq;


namespace Ocell
{
    public static class SecondaryTiles
    {
        public static bool ComposeTileIsCreated()
        {
            if (ShellTile.ActiveTiles.Count() == 0)
                return false;
            ShellTile ComposeTile = ShellTile.ActiveTiles.FirstOrDefault(item => item != null
                && !string.IsNullOrWhiteSpace(item.NavigationUri.ToString()) &&
                item.NavigationUri.ToString().Contains("NewTweet.xaml"));
            return ComposeTile != null;
        }

        public static bool ColumnTileIsCreated(TwitterResource Resource)
        {
            ShellTile ColumnTile = ShellTile.ActiveTiles.FirstOrDefault(item => item != null
                && !string.IsNullOrWhiteSpace(item.NavigationUri.ToString()) &&
                item.NavigationUri.ToString().Contains(Uri.EscapeDataString(Resource.String)));
            return ColumnTile != null;
        }

        public static void CreateComposeTile()
        {
            if (ComposeTileIsCreated())
                return;

            StandardTileData ComposeTile = new StandardTileData
            {
                Title = Localization.Resources.NewTweet,
                BackgroundImage = new Uri("/Images/ComposeTile.png", UriKind.Relative)
            };

            ShellTile.Create(new Uri("/Pages/NewTweet.xaml", UriKind.Relative), ComposeTile);
        }

        private static string GetTitle(TwitterResource Resource)
        {
            ListConverter Converter = new ListConverter();

            string Title = (string)Converter.Convert(Resource.String, null, null, null);
            Title = char.ToUpper(Title[0]) + Title.Substring(1);

            return Title;
        }

        public static void CreateColumnTile(TwitterResource Resource)
        {
            if (Resource == null || ColumnTileIsCreated(Resource))
                return;

            StandardTileData ColumnTile = new StandardTileData
            {
                Title = GetTitle(Resource),
                BackgroundImage = new Uri("/Images/ColumnTile.png", UriKind.Relative)
            };

            Uri ColumnUri = new Uri("/MainPage.xaml?column=" + Uri.EscapeDataString(Resource.String), UriKind.Relative);

            ShellTile.Create(ColumnUri, ColumnTile);
        }
    }
}
