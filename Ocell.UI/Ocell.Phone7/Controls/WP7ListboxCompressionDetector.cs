using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TweetSharp;

namespace Ocell.Controls
{
    public class WP7ListboxCompressionDetector : IListboxCompressionDetector
    {
        ExtendedListBox listbox;

        bool viewportChanged = false;
        bool isMoving = false;
        double manipulationStart = 0;
        double manipulationEnd = 0;
        Grid topGrid = new Grid();
        Grid bottomGrid = new Grid();

        public bool Bound { get; private set; }

        public void Bind(ExtendedListBox listbox)
        {
            Bound = true;
            this.listbox = listbox;
            listbox.ManipulationStateChanged += listbox_ManipulationStateChanged;
            listbox.MouseMove += listbox_MouseMove;
        }

        public void Unbind()
        {
            Bound = false;

            if (listbox != null)
            {
                listbox.ManipulationStateChanged -= listbox_ManipulationStateChanged;
                listbox.MouseMove -= listbox_MouseMove;
            }
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
                    if (IsAtTop())
                        Compression(this, new CompressionEventArgs(CompressionType.Top));
                    else if (IsAtBottom())
                        Compression(this, new CompressionEventArgs(CompressionType.Bottom));
                }
            }
        }

        private bool IsAtTop()
        {
            if (listbox.ItemsSource.Count == 0)
                return true;

            ContentPresenter firstContainer;
            ITweetable firstItem = listbox.ItemsSource[0] as ITweetable;

            if (listbox.ViewportItems.TryGetValue(firstItem, out firstContainer))
            {
                var diff = listbox.GetRelativePosition(firstContainer);
                if (diff.Y < 2)
                    return true;
            }

            return false;
        }

        private bool IsAtBottom()
        {
            if (listbox.ItemsSource.Count == 0)
                return true;

            ContentPresenter lastContainer;
            ITweetable lastItem = listbox.ItemsSource[listbox.ItemsSource.Count - 1] as ITweetable;

            if (listbox.ViewportItems.TryGetValue(lastItem, out lastContainer))
            {
                var diff = lastContainer.GetRelativePosition(listbox);
                if (diff.Y <= listbox.ActualHeight - lastContainer.ActualHeight + 2)
                    return true;
            }

            return false;
        }


        public event OnCompression Compression;
    }
}
