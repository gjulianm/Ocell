using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocell.Controls
{
    public interface IScrollController
    {
        void Bind(ExtendedListBox list);
        void LoadCalled(int position = -1);
        void Unbind();
        bool Bound { get; set; }
    }

}
