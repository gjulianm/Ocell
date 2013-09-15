using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Ocell
{
    public class HasMediaToBackgroundConverter : IValueConverter
    {
        private static TextToMediaConverter innerConverter = new TextToMediaConverter();
        private static SolidColorBrush transparentBlack = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
        private static SolidColorBrush transparent = new SolidColorBrush(Colors.Transparent);
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var hasImage = innerConverter.Convert(value, targetType, parameter, culture) != null;

            if (hasImage)
                return transparentBlack;
            else
                return transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
