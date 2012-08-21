using System;
using DanielVaughan;
using DanielVaughan.InversionOfControl;
using DanielVaughan.Net;
using DanielVaughan.Services;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocell;
using DanielVaughan.InversionOfControl.Containers.SimpleContainer;
using Ocell.Library;
using Ocell.Library.Twitter;

namespace Ocell.Testing.Tests.UI.Drafts
{
    [TestClass]
    [Tag("UI")]
    public class ManageDraftsTest
    {
        private ManageDraftsModel viewModel;
        private string firstURL;
        [TestInitialize]
        public void TestInitialize()
        {
            Config.Drafts.Add(new TwitterDraft { Text = "AsdAsdAsd" });
            Config.Drafts.Add(new TwitterDraft { Text = "DSADSA" });

            viewModel = new ManageDraftsModel();
            firstURL = "URLStart";
            Dependency.Resolve<INavigationService>().Navigate(firstURL);
            Dependency.Resolve<INavigationService>().Navigate("Drafts");
        }

        [TestMethod]
        [Description("Test draft deletion")]
        public void TestRemoval()
        {
            var grid = new System.Windows.Controls.Grid();
            var draft = Config.Drafts[0];
            grid.Tag = draft;
            viewModel.GridHold(grid, new System.Windows.Input.GestureEventArgs());
            Assert.IsFalse(viewModel.Collection.Contains(draft));
        }

        [TestMethod]
        [Description("Test navigation")]
        public void TestNavigation()
        {
            var draft = Config.Drafts[0];
            viewModel.ListSelection = draft;
            Assert.AreEqual(draft, DataTransfer.Draft);
            Assert.AreEqual(firstURL, Dependency.Resolve<INavigationService>().Source.ToString());
        }


    }
}
