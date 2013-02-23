using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocell.Controls
{
    public class WP8ReadingPositionManager : IReadingPositionManager
    {
        bool Bound { get; }
        void Bind(ExtendedListBox listbox);
        void SavePosition();
        bool CanRecoverPosition();
        void RecoverPosition();
    }
}
