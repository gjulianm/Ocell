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
using System.Windows.Media.Imaging;

namespace Ocell.Library
{
    public enum LightOrDark { SystemDefault, Light, Dark, };
    public enum BackgroundType { ThemeDependant = 0, None = 1, BlackFabric = 2, GrayEgg = 3, BlackTiled = 4, Tire = 5, Floral = 6, Map = 7, Diamond = 8 };

    public class OcellTheme
    {
        public BackgroundType Background { get; set; }
        public LightOrDark Type
        {
            get
            {
                if (Background == BackgroundType.BlackFabric ||
                    Background == BackgroundType.BlackTiled
                    || Background == BackgroundType.Tire
                    || Background == BackgroundType.Map)
                    return LightOrDark.Dark;
                else if (Background == BackgroundType.GrayEgg
                    || Background == BackgroundType.Floral
                    || Background == BackgroundType.Diamond)
                    return LightOrDark.Light;
                else
                    return LightOrDark.SystemDefault;
            }
        }

        public string BackgroundUrl
        {
            get
            {
                switch (Background)
                {
                    case BackgroundType.BlackFabric:
                        return "/Images/Backgrounds/Fabric.png";
                    case BackgroundType.BlackTiled:
                        return "/Images/Backgrounds/BlackTiled.png";
                    case BackgroundType.GrayEgg:
                        return "/Images/Backgrounds/Gray.png";
                    case BackgroundType.None:
                        return "";
                    case BackgroundType.ThemeDependant:
                        bool isDarkTheme = ((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible);
                        if (isDarkTheme)
                            return "/Images/Backgrounds/Fabric.png";
                        else
                            return "/Images/Backgrounds/Gray.png";
                    case BackgroundType.Tire:
                        return "/Images/Backgrounds/Tire.png";
                    case BackgroundType.Floral:
                        return "/Images/Backgrounds/Floral.png";
                    case BackgroundType.Map:
                        return "/Images/Backgrounds/Map.png";
                    case BackgroundType.Diamond:
                        return "/Images/Backgrounds/Diamond.png";
                    default:
                        return "";
                }
            }
        }

        public Brush GetBrush()
        {
            if (BackgroundUrl == "")
                return new SolidColorBrush(Colors.Transparent);
            else
            {
                return new ImageBrush { ImageSource = new BitmapImage(new Uri(BackgroundUrl, UriKind.Relative)) };
            }
        }

        void z_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            e.Equals(1);
        }
    }


}
