using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;

namespace Ocell
{
    public partial class ImageView : PhoneApplicationPage
    {
        public ImageView()
        {
            InitializeComponent();

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
                catch (Exception ex)
                {
                    Exception e2 = ex;
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
