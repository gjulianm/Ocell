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
using DanielVaughan.Services;
using System.Threading;
using Microsoft.Phone.Shell;

namespace Ocell.Library
{
    public static class MessageServiceExtension
    {
        public static void ShowLightNotification(this IMessageService service, string message)
        {
            var dispatcher = Deployment.Current.Dispatcher;

            dispatcher.BeginInvoke(() =>
                {
                    var bar = SystemTray.ProgressIndicator;
                    if (bar == null)
                        return;
                    bar.IsIndeterminate = false;
                    bar.Value = 0;
                    bar.Text = message;
                    bar.IsVisible = true;
                });

            var timer = new Timer((context) =>
                {
                    dispatcher.BeginInvoke(() =>
                        {
                            var bar = SystemTray.ProgressIndicator;
                            if (bar == null)
                                return;
                            bar.IsVisible = false;
                            bar.IsIndeterminate = true;
                            bar.Text = "";
                        });
                }, null, 3500, Timeout.Infinite);

        }

        public static void SetLoadingBar(this IMessageService service, bool isLoading, string message = "")
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var bar = SystemTray.ProgressIndicator;
                if (bar != null)
                {
                    bar.IsIndeterminate = true;
                    bar.IsVisible = isLoading;
                    bar.Text = message;
                }
            });
        }
    }
}
