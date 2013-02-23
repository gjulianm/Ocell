using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocell.Controls
{
    public interface IInfiniteScroller
    {
        bool Bound { get; }
        void Bind(ExtendedListBox lb);
    }
}
