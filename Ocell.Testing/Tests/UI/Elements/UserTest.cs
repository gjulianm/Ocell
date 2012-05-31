using System;
using DanielVaughan;
using DanielVaughan.InversionOfControl;
using DanielVaughan.Net;
using DanielVaughan.Services;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocell.Pages.Search;
using DanielVaughan.InversionOfControl.Containers.SimpleContainer;


namespace Ocell.Testing.Tests.UI.Elements
{
    [TestClass]
    [Tag("UI")]
    public class UserTest
    {
        private UserTest viewModel;

        [TestInitialize]
        public void TestInitialize()
        {
            viewModel = new UserTest();
        }

        // Untestable until MockTwitterService is created.
    }
}
