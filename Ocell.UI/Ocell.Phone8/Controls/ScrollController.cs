using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocell.Controls
{
    /// <summary>
    /// WP8 doesn't need a scroll controller, so just create a dummy one.
    /// </summary>
    public class DummyScrollController : IScrollController
    {
        public bool Bound { get; set; }

        public void Bind(ExtendedListBox list)
        {
            Bound = true;
        }

        public void Unbind()
        {
        }

        public void LoadCalled(int position = -1)
        {
        }
    }
}
