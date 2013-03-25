using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using LinqToVisualTree;
using Ocell.Library;
using Ocell.Library.Twitter;
using System.Windows;
using DanielVaughan;
namespace Ocell.Controls
{
    public class WP7ReadingPositionManager : IReadingPositionManager
    {
        ScrollViewer scrollViewer;
        ExtendedListBox lb;

        public bool Bound { get; private set; }

        public void Bind(ExtendedListBox listbox)
        {
            lb = listbox;

            Deployment.Current.Dispatcher.InvokeIfRequired(() => { 
                scrollViewer = listbox.Descendants().OfType<ScrollViewer>().FirstOrDefault();
                lb.ManipulationCompleted += lb_ManipulationCompleted;
            });

                     
            Bound = true;
        }

        public void Unbind()
        {
            var tmpLb = lb;
            scrollViewer = null;
            lb = null;
            Deployment.Current.Dispatcher.InvokeIfRequired(() => tmpLb.ManipulationCompleted -= lb_ManipulationCompleted);
            
            Bound = false;
        }

        void lb_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            SavePosition();
        }  

        public void SavePosition()
        {
            if (scrollViewer == null)
                return;

            var elementOffset = (int)scrollViewer.VerticalOffset;

            if (elementOffset < lb.Loader.Source.Count)
            {
                var element = lb.Loader.Source.OrderByDescending(x => x.Id).ElementAt(elementOffset);
                Config.ReadPositions[lb.Loader.Resource.String] = element.Id;
                Config.SaveReadPositions();
            }
        }

        public bool CanRecoverPosition()
        {
            long id;
            if (Config.ReadPositions.TryGetValue(lb.Loader.Resource.String, out id)
                && lb.Loader.Source.Any(item => item.Id == id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RecoverPosition()
        {
            long id;
            if (!Config.ReadPositions.TryGetValue(lb.Loader.Resource.String, out id))
                return;

            var item = lb.Loader.Source.FirstOrDefault(x => x.Id == id);

            if (item != null && false)
                Deployment.Current.Dispatcher.InvokeIfRequired(() => lb.ScrollTo(item));
        }
    }
}
