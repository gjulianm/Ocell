using Ocell.Library;
using System;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using TweetSharp;

namespace Ocell
{
    public class TextToMediaConverter : IValueConverter
    {
        private static MediaLinkParser parser = new MediaLinkParser();

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var status = value as TwitterStatus;
            string imageLink = null;

            if (status != null && status.Entities != null)
            {
                if (status.Entities.Media.Any())
                {
                    imageLink = status.Entities.Media.First().ExpandedUrl;
                }
                else
                {
                    foreach (var url in status.Entities.Urls)
                        if (parser.TryGetMediaUrl(url.ExpandedValue, out imageLink))
                            break;
                }
            }

            Uri imageUri;

            if (imageLink != null && Uri.TryCreate(imageLink, UriKind.Absolute, out imageUri))
                return new BitmapImage(imageUri);
            else
                return null;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
