﻿
namespace Ocell.Library
{
    public enum LightOrDark { SystemDefault, Light, Dark, };
    public enum BackgroundType { ThemeDependant = 0, None = 1, BlackFabric = 2, GrayEgg = 3, BlackTiled = 4, Tire = 5, Floral = 6, Map = 7, Diamond = 8 };

    public class OcellTheme
    {
        public static bool IsDarkThemeSet { get; set; }

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
                        if (IsDarkThemeSet)
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
    }


}
