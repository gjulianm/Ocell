using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Ocell
{
    public class HasMediaToBackgroundConverter : IValueConverter
    {
        private static TextToMediaConverter innerConverter = new TextToMediaConverter();
        private static SolidColorBrush transparentBlack = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
        private static SolidColorBrush transparentWhite = new SolidColorBrush(Color.FromArgb(100, 200, 200, 200));
        private static SolidColorBrush transparent = new SolidColorBrush(Colors.Transparent);
        private static bool? _isDarkTheme;

        private bool IsDarkTheme
        {
            get
            {
                if (_isDarkTheme == null)
                    _isDarkTheme = ((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible);

                return (bool)_isDarkTheme;
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (IsDarkTheme)
                return transparentBlack;
            else if (!IsDarkTheme)
                return transparentWhite;
            else
                return transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
