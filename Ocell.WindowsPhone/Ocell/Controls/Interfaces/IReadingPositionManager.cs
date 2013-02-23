﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocell.Controls
{
    public interface IReadingPositionManager
    {
        bool Bound { get; }
        void Bind(ExtendedListBox listbox);
        void SavePosition();
        bool CanRecoverPosition();
        void RecoverPosition();
    }
}