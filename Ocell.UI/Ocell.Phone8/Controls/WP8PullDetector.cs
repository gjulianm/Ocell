using Microsoft.Phone.Controls;
using System;
using System.Windows.Controls.Primitives;

namespace Ocell.Controls
{
    /// <summary>
    /// This class detects the pull gesture on a LongListSelector. How does it work?
    /// 
    ///     This class listens to the change of manipulation state of the LLS, to the MouseMove event 
    ///     (in WP, this event is triggered when the user moves the finger through the screen)
    ///     and to the ItemRealized/Unrealized events.
    ///     
    ///     Listening to MouseMove, we can calculate the amount of finger movement. That is, we can 
    ///     detect when the user has scrolled the list.
    ///     
    ///     Then, when the ManipulationState changes from Manipulating to Animating (from user 
    ///     triggered movement to inertia movement), we check the viewport changes. The viewport is 
    ///     only constant when the user scrolls beyond the end of the list, either at the top or at the bottom.
    ///     If no items were added, check the direction of the scroll movement and fire the corresponding event.
    /// </summary>
    public class WP8PullDetector : IListboxCompressionDetector
    {
        LongListSelector listbox;

        bool viewportChanged = false;
        bool isMoving = false;
        double manipulationStart = 0;
        double manipulationEnd = 0;

        public bool Bound { get; private set; }

        public void Bind(LongListSelector listbox)
        {
            Bound = true;
            this.listbox = listbox;
            listbox.ManipulationStateChanged += listbox_ManipulationStateChanged;
            listbox.MouseMove += listbox_MouseMove;
            listbox.ItemRealized += OnViewportChanged;
            listbox.ItemUnrealized += OnViewportChanged;
        }

        public void Unbind()
        {
            Bound = false;

            if (listbox != null)
            {
                listbox.ManipulationStateChanged -= listbox_ManipulationStateChanged;
                listbox.MouseMove -= listbox_MouseMove;
                listbox.ItemRealized -= OnViewportChanged;
                listbox.ItemUnrealized -= OnViewportChanged;
            }
        }

        void OnViewportChanged(object sender, Microsoft.Phone.Controls.ItemRealizationEventArgs e)
        {
            viewportChanged = true;
        }

        void listbox_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var pos = e.GetPosition(null);

            if (!isMoving)
                manipulationStart = pos.Y;
            else
                manipulationEnd = pos.Y;

            isMoving = true;
        }

        void listbox_ManipulationStateChanged(object sender, EventArgs e)
        {
            if (listbox.ManipulationState == ManipulationState.Idle)
            {
                isMoving = false;
                viewportChanged = false;
            }
            else if (listbox.ManipulationState == ManipulationState.Manipulating)
            {
                viewportChanged = false;
            }
            else if (listbox.ManipulationState == ManipulationState.Animating)
            {
                var total = manipulationStart - manipulationEnd;

                if (!viewportChanged && Compression != null)
                {
                    if (total < 0)
                        Compression(this, new CompressionEventArgs(CompressionType.Top));
                    else if(total > 0) // Explicitly exclude total == 0 case
                        Compression(this, new CompressionEventArgs(CompressionType.Bottom));
                }
            }
        }

       
        public event OnCompression Compression;
    }
}
