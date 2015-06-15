using AncoraMVVM.Base.IoC;
using NUnit.Framework;

namespace Ocell.ViewModels.Tests
{
    [TestFixture]
    public class ManageListsModelTests
    {
        [TestFixtureSetUp]
        public static void Setup()
        {
            var dependencyProvider = new MockProvider();
            Dependency.Provider = dependencyProvider;
        }

        [TestFixtureTearDown]
        public static void Teardown()
        {
            Dependency.Provider = null;
        }

        [Test]
        public async void GetListsForUser_InvalidUser_ReturnsEmptyList()
        {
            var model = new ManageListsModel();

            var lists = await model.GetListsForUser(null);

            Assert.IsEmpty(lists);
        }

        [Test]
        public async void GetListsForUser_ValidUser_ReturnsTheLists()
        {
            var model = new ManageListsModel();

            var lists = await model.GetListsForUser(TestCredentials.GetTwitterUser());

            Assert.IsNotEmpty(lists);
        }
    }
}
