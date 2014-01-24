﻿using System;
using System.Threading;

namespace Ocell
{
    public static class MessageServiceExtension
    {
        public static void ShowLightNotification(this IMessageService service, string message)
        {
            var dispatcher = Deployment.Current.Dispatcher;

            dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        var bar = SystemTray.ProgressIndicator;
                        if (bar == null)
                            return;
                        bar.IsIndeterminate = false;
                        bar.Value = 0;
                        bar.Text = message;
                        bar.IsVisible = true;
                    }
                    catch (Exception)
                    {
                        // On testing, Exceptions are thrown. So, if testing, don't do anything.
                    }
                });

            var timer = new Timer((context) =>
                {
                    dispatcher.BeginInvoke(() =>
                        {
                            try
                            {

                                var bar = SystemTray.ProgressIndicator;
                                if (bar == null)
                                    return;
                                bar.IsVisible = false;
                                bar.IsIndeterminate = true;
                                bar.Text = "";
                            }
                            catch (Exception)
                            {
                                // same as above.
                            }
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
