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
using System.ComponentModel;
using Ocell.Library;

namespace Ocell
{
    public class GlobalSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName)
        {
            var dispatcher = Deployment.Current.Dispatcher;
            if (!dispatcher.CheckAccess())
                dispatcher.BeginInvoke(() => RaisePropertyChanged(propertyName));
            else
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        const int DefaultTweetFontSize = 20;
        public int TweetFontSize
        {
            get { return Config.FontSize ?? DefaultTweetFontSize; }
            set
            {
                if (Config.FontSize == value)
                    return;

                Config.FontSize = value;

                RaisePropertyChanged("TweetFontSize");
            }
        }
    }
}
