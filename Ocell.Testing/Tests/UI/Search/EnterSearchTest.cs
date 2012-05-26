using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocell.Pages.Search;
using DanielVaughan;
using DanielVaughan.Services;
using DanielVaughan.Services.Implementation;

namespace Ocell.Testing.Tests.UI.Search
{
    [TestClass]
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
    }
}
