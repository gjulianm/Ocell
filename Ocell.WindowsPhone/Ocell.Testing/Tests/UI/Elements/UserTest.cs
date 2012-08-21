using System;
using DanielVaughan;
using DanielVaughan.InversionOfControl;
using DanielVaughan.Net;
using DanielVaughan.Services;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocell.Pages.Elements;
using DanielVaughan.InversionOfControl.Containers.SimpleContainer;
using TweetSharp;
using DanielVaughan.Services.Implementation;
using Ocell.Library;
using Ocell.Library.Twitter;

namespace Ocell.Testing.Tests.UI.Elements
{
    [TestClass]
    [Tag("UI")]
    public class UserTest
    {
        private UserModel viewModel;

        [TestInitialize]
        public void TestInitialize()
        {
            Config.Accounts.Clear();
            ServiceDispatcher.TestSession = true;
            DataTransfer.CurrentAccount = new Ocell.Library.Twitter.UserToken { Key = "asdsdda", ScreenName = "Test", Secret = "lklkl", Id = 1398901238 };
            Config.Accounts.Add(DataTransfer.CurrentAccount);
            viewModel = new UserModel();
        }

        [TestMethod]
        public void ReceiveUser()
        {
            // Test with correct answers.
            MockTwitterService.ReturnsFail = false;
            viewModel.Loaded("test"); // No exception => Good. 
        }

        [TestMethod]
        [Description("Test blocking")]
        public void TestBlock()
        {
            viewModel.Loaded("asd");
            Assert.IsNotNull(viewModel.User); // Initial condition.

            var previous = viewModel.Blocked;
            viewModel.Block.Execute(null);
            Assert.AreNotEqual(viewModel.Blocked, previous); // Test it switches.
        }

        [TestMethod]
        [Description("Test change avatar.")]
        public void ChangeAvatar()
        {
            string name = "testName";
            Config.Accounts.Add(new Ocell.Library.Twitter.UserToken { ScreenName = name });
            MockTwitterService.NextId = (int)DataTransfer.CurrentAccount.Id;
            viewModel.Loaded(name);
            Assert.IsTrue(viewModel.IsOwner);
            Assert.IsTrue(viewModel.ChangeAvatar.CanExecute(null));
            viewModel.Loaded("anotherName");
            Assert.IsFalse(viewModel.IsOwner);
            Assert.IsFalse(viewModel.ChangeAvatar.CanExecute(null));
        }

        [TestMethod]
        [Description("Test spam report.")]
        public void Report()
        {
            viewModel.Loaded("asd");
            viewModel.ReportSpam.Execute(null); // No exception => Nice :)
        }

        [TestMethod]
        [Description("Test following")]
        public void Following()
        {
            // just as in blocking. Follow and Unfollow share the same structure.
            viewModel.Loaded("asd");
            Assert.IsNotNull(viewModel.User); // Initial condition.

            var previous = viewModel.Followed;
            viewModel.FollowUser.Execute(null);
            Assert.AreNotEqual(viewModel.Followed, previous); // Test it switches.
        }

        [TestMethod]
        [Description("Test pinning")]
        public void TestPin()
        {
            viewModel.Loaded("asd");
            Assert.IsTrue(viewModel.PinUser.CanExecute(null));
            viewModel.PinUser.Execute(null);
            Assert.IsFalse(viewModel.PinUser.CanExecute(null));
        }


    }
}
