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
using Microsoft.Phone.Controls;
using System.Windows.Interactivity;

namespace Ocell.Transitions
{
    public class OnLoadedOpacityTransitionBehavior : Behavior<FrameworkElement>
    {

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObjectLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObjectLoaded;
        }

        private void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            var opacityTransition = new OpacityTransitionElement();
            var transition = opacityTransition.GetTransition(sender as UIElement);
            transition.Begin();
        }

    }
}
