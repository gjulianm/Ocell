using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocell.Controls
{
    public class WP7ListboxCompressionDetector : IListboxCompressionDetector
    {
        LongListSelector list;

        public bool Bound { get; private set; }

        public void Bind(Microsoft.Phone.Controls.LongListSelector listbox)
        {
            list = listbox;
            list.StretchingBottom += list_StretchingBottom;
            list.StretchingTop += list_StretchingTop;

            Bound = true;
        }

        void list_StretchingTop(object sender, EventArgs e)
        {
            if (Compression != null)
                Compression(this, new CompressionEventArgs(CompressionType.Top));
        }

        void list_StretchingBottom(object sender, EventArgs e)
        {

            if (Compression != null)
                Compression(this, new CompressionEventArgs(CompressionType.Bottom));
        }

        public void Unbind()
        {
            if (list != null)
            {
                list.StretchingBottom -= list_StretchingBottom;
                list.StretchingTop -= list_StretchingTop;
            }

            Bound = false;
        }

        public event OnCompression Compression;
    }
}
