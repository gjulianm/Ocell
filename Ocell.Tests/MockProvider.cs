using AncoraMVVM.Testing;
using NSubstitute;

namespace Ocell.Tests
{
    public class MockProvider : BaseMockProvider
    {
        public override T GetSubstituteFor<T>()
        {
            return Substitute.For<T>();
        }

        public MockProvider()
        {
            Dispatcher.IsUIThread.Returns(true);
        }
    }
}
