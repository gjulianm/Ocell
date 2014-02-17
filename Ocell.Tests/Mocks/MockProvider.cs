using AncoraMVVM.Base.Files;
using AncoraMVVM.Testing;
using NSubstitute;
using Ocell.Tests.Mocks;

namespace Ocell.Tests
{
    public class MockProvider : BaseMockProvider
    {
        public override T GetSubstituteFor<T>()
        {
            if (typeof(T) == typeof(IFileManager))
                return (T)(object)new MockFileManager();
            else
                return Substitute.For<T>();
        }

        public MockProvider()
        {
            Dispatcher.IsUIThread.Returns(true);
        }
    }
}
