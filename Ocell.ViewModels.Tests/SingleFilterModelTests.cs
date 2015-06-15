using AncoraMVVM.Base.IoC;
using NSubstitute;
using NUnit.Framework;
using Ocell.Library;
using Ocell.Library.Filtering;
using Ocell.Localization;
using Ocell.Tests.TestHelpers.Events;
using Ocell.Tests.TestHelpers.Extensions;
using System;

namespace Ocell.ViewModels.Tests
{
    [TestFixture]
    public class SingleFilterModelTests
    {
        [TestFixtureSetUp]
        public static void Setup()
        {
            var dependencyProvider = new MockProvider();
            dependencyProvider.ConfigurationManager.Get(Config.DefaultMuteTime).Returns(TimeSpan.FromHours(8));
            Dependency.Provider = dependencyProvider;
        }

        [TestFixtureTearDown]
        public static void Teardown()
        {
            Dependency.Provider = null;
        }

        [Test]
        public void FilterDescription_SelectedFilterTypeChanges_Updated()
        {
            var model = new SingleFilterModel();
            var testType = "TESTSTRING";

            model.ShouldNotifyOn(x => x.FilterDescription).When(x => x.SelectedFilterType = testType);

            string expectedDescription = String.Format(Resources.TweetsWhere, testType);
            model.SelectedFilterType = testType;
            Assert.AreEqual(expectedDescription, model.FilterDescription);
        }

        [Test]
        public void FilterType_SelectedFilterTypeChanges_Updated()
        {
            var model = new SingleFilterModel();
            var testType = "TESTSTRING";

            model.ShouldNotifyOn(x => x.FilterType).When(x => x.SelectedFilterType = testType);
        }

        [Test]
        public void FilterType_InvalidFilterType_ReturnsNull()
        {
            var model = new SingleFilterModel();
            var testType = "TESTSTRING";

            model.SelectedFilterType = testType;
            Assert.IsNull(model.FilterType);
        }

        [Test]
        public void FilterType_ValidFilterType_ReturnsFilters()
        {
            var model = new SingleFilterModel();

            foreach (var typeString in model.FilterTypes)
            {
                model.SelectedFilterType = typeString;
                Assert.IsNotNull(model.FilterType);
                Assert.IsTrue(model.FilterType.IsSubclassOfRawGeneric(typeof(ElementFilter<>)));
            }
        }

        [Test]
        public void SaveCommand_FilterTextChanged_Updated()
        {
            var model = new SingleFilterModel();

            model.SaveCommand.ShouldChangeCanExecuteWhen(() => model.FilterText = "AAA");
        }

        [Test]
        public void SaveCommand_FilterTextNullOrEmpty_CannotExecute()
        {
            var model = new SingleFilterModel();

            model.FilterText = null;
            Assert.IsFalse(model.SaveCommand.CanExecute(null));

            model.FilterText = "";
            Assert.IsFalse(model.SaveCommand.CanExecute(null));

            model.FilterText = "   ";
            Assert.IsFalse(model.SaveCommand.CanExecute(null));
        }

        [Test]
        public void SaveCommand_SelectedFilterTypeChanged_Updated()
        {
            var model = new SingleFilterModel();

            model.SaveCommand.ShouldChangeCanExecuteWhen(() => model.SelectedFilterType = "AAA");
        }

        [Test]
        public void SaveCommand_InvalidFilterType_CannotExecute()
        {
            var model = new SingleFilterModel();
            model.FilterText = "asd";
            model.SelectedFilterType = "NotAValidFilterType";
            Assert.IsFalse(model.SaveCommand.CanExecute(null));
        }

        [Test]
        public void GenerateFilter_ValidFilterType_CanGenerate()
        {
            var model = new SingleFilterModel();

            model.SelectedFilterType = Resources.author;

            var filter = model.GenerateFilter();

            Assert.IsNotNull(filter);
            Assert.IsInstanceOf(typeof(UserFilter), filter);
        }

        [Test]
        public void SaveCommand_CustomDateBeforeNow_CannotExecute()
        {
            var model = new SingleFilterModel();

            model.SelectedFilterTime = Resources.CustomDate;
            model.CustomDateTime = DateTime.Now.AddYears(-1);
            Assert.IsFalse(model.SaveCommand.CanExecute(null));
        }

        [Test]
        public void SaveCommand_CustomDateBeforeNowAndNotCustomDateChosen_CanExecute()
        {
            var model = new SingleFilterModel();
            model.SelectedFilterType = model.FilterTypes[0];
            model.FilterText = "asd";
            model.SelectedFilterTime = Resources.OneHour;
            model.CustomDateTime = DateTime.Now.AddYears(-1);
            Assert.IsTrue(model.SaveCommand.CanExecute(null));
        }
    }
}
