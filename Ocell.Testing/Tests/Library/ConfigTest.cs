using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocell.Library;
using DanielVaughan;
using DanielVaughan.Services;
using DanielVaughan.Services.Implementation;
using System.Threading;

namespace Ocell.Testing.Tests.Library
{
    [TestClass]
    [Tag("Library")]
    public class ConfigTest
    {
        private Exception _tWriteEx;
        private Exception _tReadEx;

        private int _readValue;

        [TestInitialize]
        public void ConfigTestInitialize()
        {
            Config.ClearAll();
        }

        [TestMethod]
        [Description("Test normal access")]
        public void TestNormalAccess()
        {
            int value = 30;

            // Use one random value, as they share the same code.
            Assert.IsNull(Config.TweetsPerRequest);
            Config.TweetsPerRequest = value;
            Assert.AreEqual(Config.TweetsPerRequest, value);
        }

        [TestMethod]
        [Description("Test threaded access")]
        [Tag("Threading")]
        public void TestThreadedAccess()
        {
            int timeout = 1000;
            int valueToWrite1 = 3;
            int valueToWrite2 = 5;

            Assert.AreNotEqual(valueToWrite1, _readValue);
            Assert.AreNotEqual(valueToWrite2, _readValue);
            Assert.AreNotEqual(valueToWrite1, valueToWrite2);

            _tWriteEx = null;
            _tReadEx = null;

            Thread threadWrite1 = new Thread(new ParameterizedThreadStart(WriteConfig));
            Thread threadWrite2 = new Thread(new ParameterizedThreadStart(WriteConfig));
            Thread threadRead = new Thread(new ThreadStart(ReadConfig));

            threadWrite1.Start(valueToWrite1);
            Thread.Sleep(100);
            threadWrite2.Start(valueToWrite2);
            threadRead.Start();

            bool threadWrite1Result = threadWrite1.Join(timeout);
            bool threadWrite2Result = threadWrite2.Join(timeout);
            bool threadReadResult = threadRead.Join(timeout);

            Assert.IsTrue(threadWrite1Result);
            Assert.IsTrue(threadWrite2Result);
            Assert.IsTrue(threadReadResult);

            Assert.IsNull(_tWriteEx);
            Assert.IsNull(_tReadEx);
        }

        private void WriteConfig(object data)
        {
            try
            {
                int toWrite = (int)data;
                Config.TweetsPerRequest = toWrite;
            }
            catch (Exception e)
            {
                _tWriteEx = e;
            }
        }

        private void ReadConfig()
        {
            try
            {
                _readValue = Config.TweetsPerRequest.Value;
            }
            catch (Exception e)
            {
                _tReadEx = e;
            }
        }
    }
}
