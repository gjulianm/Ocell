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
using Ocell.Pages;
using System.Linq;
using TweetSharp;

namespace Ocell.Testing.Tests.UI.DirectMessage
{
    [TestClass]
    [Tag("UI")]
    public class SelectUserTest
    {
        private SelectUserModel viewModel;

        [TestInitialize]
        public void TestInitialize()
        {
            Config.Accounts.Add(new UserToken { ScreenName = "test1" });
            Config.Accounts.Add(new UserToken { ScreenName = "test2" });
            viewModel = new SelectUserModel();
            viewModel.Loaded();
        }

        [TestMethod]
        [Description("Check for button activation")]
        public void CheckActivation()
        {
            viewModel.Sender = null;
            viewModel.Destinatary = null;
            Assert.IsFalse(viewModel.GoNext.CanExecute(null));
            viewModel.Sender = viewModel.Accounts.First() ;
            Assert.IsFalse(viewModel.GoNext.CanExecute(null));
            viewModel.Destinatary = new TwitterUser();
            Assert.IsTrue(viewModel.GoNext.CanExecute(null));
        }

    }
}
