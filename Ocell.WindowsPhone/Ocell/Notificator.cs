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
using Microsoft.Phone.Shell;
using System.Threading;

namespace Ocell
{
    public static class Notificator
    {
        public static void ShowMessage(string msg, ProgressIndicator bar)
        {
            bar.IsIndeterminate = false;
            bar.Value = 0;
            bar.Text = msg;
            bar.IsVisible = true;
            Thread.Sleep(3500);
            bar.IsVisible = false;
            bar.IsIndeterminate = true;
            bar.Text = "";
        }
    }
}
