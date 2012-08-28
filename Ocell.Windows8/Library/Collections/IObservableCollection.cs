using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Ocell.Library.Collections
{
    public interface IObservableCollection<T> : IReadOnlyList<T>, INotifyCollectionChanged
    {
    }
}
