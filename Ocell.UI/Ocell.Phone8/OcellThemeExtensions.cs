using Ocell.Library;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ocell
{
    public static class OcellThemeExtensions
    {
        public static Brush GetBrush(this OcellTheme theme)
        {
            if (theme.BackgroundUrl == "")
                return new SolidColorBrush(Colors.Transparent);
            else
            {
                var bi = new BitmapImage(new Uri(theme.BackgroundUrl, UriKind.Relative));
                bi.CreateOptions = BitmapCreateOptions.None;
                var ib = new ImageBrush { ImageSource = bi };
                return ib;
            }
        }
    }
}
