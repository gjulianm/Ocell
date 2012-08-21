using System;
using DanielVaughan;
using DanielVaughan.InversionOfControl;
using DanielVaughan.Net;
using DanielVaughan.Services;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocell.Pages.Search;
using DanielVaughan.InversionOfControl.Containers.SimpleContainer;
using Ocell;
using Ocell.Controls;
using Ocell.Library;

namespace Ocell.Testing.Tests.UI
{
    [TestClass]
    [Tag("UI")]
    public class MainPageTest
    {
        MainPageModel viewModel;

        [TestInitialize]
        public void Init()
        {
            viewModel = new MainPageModel();
        }

        [TestMethod]
        [Description("Test reload all")]
        public void ReloadAll()
        {
            bool reloadAllCalled;
            bool broadcast;
            viewModel.ReloadLists += (sender, e) => { broadcast = e.BroadcastAll; reloadAllCalled = true; };
        }
    }
}
