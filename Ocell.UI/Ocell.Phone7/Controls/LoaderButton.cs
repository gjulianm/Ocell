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

namespace Ocell.Controls
{
    public class LoaderButton : Button
    {
        public bool isShown { get; set; }
        public Grid Container { get; set; }

        public LoaderButton()
            : base()
        {
            isShown = false;
        }

        public void Toggle()
        {
            Thickness newMargin = new Thickness();
            if (Container == null)
                return;

            newMargin.Left = Container.Margin.Left;
            newMargin.Right = Container.Margin.Right;
            newMargin.Top = Container.Margin.Top;
            newMargin.Bottom = Container.Margin.Bottom;


            if (!isShown)
            {
                newMargin.Top -= ActualHeight;
                newMargin.Bottom -= ActualHeight;
            }
            else
            {
                newMargin.Top += ActualHeight;
                newMargin.Bottom += ActualHeight;
            }
            isShown = !isShown;
            Container.Margin = newMargin;
        }
    }
}
