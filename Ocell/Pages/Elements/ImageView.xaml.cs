using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;

namespace Ocell.Pages.Elements
{
    public partial class ImageView : PhoneApplicationPage
    {
        public ImageView()
        {
            InitializeComponent(); ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);

            this.Loaded += new RoutedEventHandler(ImageView_Loaded);
        }

        void ImageView_Loaded(object sender, RoutedEventArgs e)
        {
            string url;
            Uri img;
            if (NavigationContext.QueryString.TryGetValue("img", out url))
            {
                try
                {
                    img = new Uri(url, UriKind.Absolute);
                    BigImage.Source = new BitmapImage { UriSource = img };
                }
                catch (Exception)
                {
                    ShowMessageAndGoBack();
                }
            }
            else
                ShowMessageAndGoBack();
                
        }

        void ShowMessageAndGoBack()
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show("Couldn't load the image.");
                NavigationService.GoBack();
            });
        }
    }
}
