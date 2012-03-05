using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Shell;
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

        public static bool ColumnTileIsCreated(Library.TwitterResource Resource)
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
                Title = "New tweet",
                BackgroundImage = new Uri("/Images/ComposeTile.png", UriKind.Relative)
            };

            Uri ComposeUri = new Uri("/Pages/NewTweet.xaml", UriKind.Relative);

            ShellTile.Create(ComposeUri, ComposeTile);
        }

        private static string GetTitle(Library.TwitterResource Resource)
        {
            ListConverter Converter = new ListConverter();

            string Title = (string)Converter.Convert(Resource.String, null, null, null);
            Title = char.ToUpper(Title[0]) + Title.Substring(1);

            return Title;
        }

        public static void CreateColumnTile(Library.TwitterResource Resource)
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
