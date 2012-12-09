using System;
using DanielVaughan;
using DanielVaughan.InversionOfControl;
using DanielVaughan.Net;
using DanielVaughan.Services;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocell.Pages.Search;
using DanielVaughan.InversionOfControl.Containers.SimpleContainer;
using Ocell.Library;

namespace Ocell.Testing.Tests.UI.Search
{
    [TestClass]
    [Tag("UI")]
    public class SearchTest
    {
        private ResourceViewModel viewModel;

        [TestInitialize]
        public void TestInitialize()
        {
            viewModel = new ResourceViewModel();
            Config.Columns.Clear();
        }

        [TestMethod]
        [Description("Check activation of appBar button.")]
        public void CheckButtonActivation()
        {
            string testQuery = "test";
            viewModel.PageTitle = testQuery;
            Config.Columns.Clear();
            Assert.IsTrue(viewModel.AddCommand.CanExecute(null));
            Config.Columns.Add(new Ocell.Library.Twitter.TwitterResource { Data = testQuery, Type = Ocell.Library.Twitter.ResourceType.Tweets });
            Assert.IsTrue(viewModel.AddCommand.CanExecute(null));
            Config.Columns.Add(new Ocell.Library.Twitter.TwitterResource { Data = testQuery, Type = Ocell.Library.Twitter.ResourceType.Search });
            Assert.IsFalse(viewModel.AddCommand.CanExecute(null));
        }
    }
}
