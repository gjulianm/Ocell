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
using System.Windows.Media.Imaging;
using Ocell.Library;
namespace Ocell
{


    public static class ThemeFunctions
    {
        public static Brush BackgroundBrush { get; set; }

        public static void SetBackground(Panel LayoutRoot)
        {
            if (BackgroundBrush == null)
                BackgroundBrush = Config.Background.GetBrush();

            LayoutRoot.Background = BackgroundBrush;
        }
    }
}
