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

namespace Ocell
{
    public static class ThemeFunctions
    {
        public static void ChangeBackgroundIfLightTheme(Panel LayoutRoot)
        {
            bool isDarkTheme = ((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible);
            if (!isDarkTheme)
            {
                LayoutRoot.Background = new SolidColorBrush(Colors.Transparent);
            }
        }
    }
}
