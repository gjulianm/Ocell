using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocell.Controls
{
    public class CompressionEventArgs : EventArgs
    {
        public CompressionType Type { get; protected set; }

        public CompressionEventArgs(CompressionType type)
        {
            Type = type;
        }
    }

    public enum CompressionType { Top, Bottom, Left, Right };

    public delegate void OnCompression(object sender, CompressionEventArgs e);

    public interface IListboxCompressionDetector
    {
        bool Bound { get; }
        void Bind(ExtendedListBox listbox);
        void Unbind();
        event OnCompression Compression;
    }
}
