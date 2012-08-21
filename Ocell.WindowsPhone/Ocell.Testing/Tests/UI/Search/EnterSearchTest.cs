using System;
using DanielVaughan;
using DanielVaughan.InversionOfControl;
using DanielVaughan.Net;
using DanielVaughan.Services;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocell.Pages.Search;
using DanielVaughan.InversionOfControl.Containers.SimpleContainer;


namespace Ocell.Testing.Tests.UI.Search
{
    [TestClass]
    [Tag("UI")]
    public class EnterSearchTest
    {
        private EnterSearchModel viewModel;

        [TestInitialize]
        public void TestInitialize()
        {
            viewModel = new EnterSearchModel();
        }

        [TestMethod]
        [Description("Check for correct button activation")]
        public void CheckButtonActivation()
        {
            viewModel.Query = "";
            Assert.IsFalse(viewModel.ButtonClick.CanExecute(null));
            viewModel.Query = "asd";
            Assert.IsTrue(viewModel.ButtonClick.CanExecute(null));
            viewModel.Query = "";
            Assert.IsFalse(viewModel.ButtonClick.CanExecute(null));
        }

        [TestMethod]
        [Description("Check for correct navigation")]
        public void CheckNavigation()
        {
            string query = "test#Query?Stra+an+g`ge chars";
            string queryOut;
            bool extractQuery;
            INavigationService navigator = Dependency.Resolve<INavigationService>();
            

            viewModel.Query = query;
            viewModel.ButtonClick.Execute(null);

            extractQuery = HttpUtilityExtended.ParseQueryString(navigator.Source.ToString()).TryGetValue("q", out queryOut);

            Assert.IsTrue(extractQuery);
            Assert.AreEqual(query, Uri.UnescapeDataString(queryOut));
        }
    }
}
