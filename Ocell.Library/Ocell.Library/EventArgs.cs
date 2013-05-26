using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocell.Library
{
    public class EventArgs<T> : EventArgs
    {
        public T Payload { get; protected set; }

        public EventArgs(T payload)
        {
            Payload = payload;
        }
    }
}
