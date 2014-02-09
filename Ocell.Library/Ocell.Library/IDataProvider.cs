using AncoraMVVM.Base;

namespace Ocell.Library
{
    public interface IDataProvider<T>
    {
        SafeObservable<T> DataList { get; }
        void StartRetrieval();
    }
}
