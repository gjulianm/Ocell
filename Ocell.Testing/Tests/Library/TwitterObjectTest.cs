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
using Ocell.LightTwitterService;
namespace Ocell.Testing.Tests.Library
{
    [TestClass]
    [Tag("Library")]
    [Tag("String manipulation")]
    public class TwitterObjectTest
    {
        [TestMethod]
        public void TestNonStringProps()
        {
            string[] names = { "a", "property", "name", "fourth" };
            string[] values = { "one", "value", "another", "one" };

            string contents = "";
            string value;

            for (int i = 0; i < names.Length; i++)
                contents += "\"" + names[i] + "\" : " + values[i] + ",";

            TwitterObject obj = new TwitterObject(contents);

            for (int j = 0; j < names.Length; j++)
            {
                Assert.IsTrue(obj.TryGetProperty(names[j], out value));
                Assert.AreEqual(values[j], value);
            }
        }

        [TestMethod]
        public void TestStringProps()
        {
            string[] names = { "a", "property", "name", "fourth" };
            string[] values = { "one", "value", "another", "one" };

            string contents="";
            string value;

            for (int i = 0; i < names.Length; i++)
                contents += "\"" + names[i] + "\" : \"" + values[i] + "\",";

            TwitterObject obj = new TwitterObject(contents);

            for (int j = 0; j < names.Length; j++)
            {
                Assert.IsTrue(obj.TryGetProperty(names[j], out value));
                Assert.AreEqual(values[j], value);
            }
        }

        [TestMethod]
        public void TestPropsWithStrangeChars()
        {
            string[] names = { "a", "property", "name", "fourth" };
            string[] values = { "\\{\\}", "\\\";;", "Should\\\"\\{", "work}{}{}" };

            string contents = "";
            string value;

            for (int i = 0; i < names.Length; i++)
                contents += "\"" + names[i] + "\" : \"" + values[i] + "\",";

            TwitterObject obj = new TwitterObject(contents);

            for (int j = 0; j < names.Length; j++)
            {
                Assert.IsTrue(obj.TryGetProperty(names[j], out value));
                Assert.AreEqual(values[j], value);
            }
        }

        [TestMethod]
        public void TestPropsWithNestedJSON()
        {
            string[] names = { "a", "property" };
            string[] values = { "json inside", "With Twiddlly_ bits and \\\" chars " };
            string contents = "";
            string value;

            for (int i = 0; i < names.Length; i++)
                contents += "\"" + names[i] + "\" : {" + values[i] + "},";

            TwitterObject obj = new TwitterObject(contents);

            for (int j = 0; j < names.Length; j++)
            {
                Assert.IsTrue(obj.TryGetProperty(names[j], out value));
                Assert.AreEqual(values[j], value);
            }
        }

        [TestMethod]
        public void TestPropsWithMultipleNestedJSONs()
        {
            string[] names = { "a", "property", "name", "fourth" };
            string[] values = { "json inside", "{even},{more},{json}", "{{wow}}", "This{should}{do{the}}{trick}" };

            string contents = "";
            string value;

            for (int i = 0; i < names.Length; i++)
                contents += "\"" + names[i] + "\" : [" + values[i] + "],";

            TwitterObject obj = new TwitterObject(contents);

            for (int j = 0; j < names.Length; j++)
            {
                Assert.IsTrue(obj.TryGetProperty(names[j], out value));
                Assert.AreEqual(values[j], value);
            }
        }

        [TestMethod]
        public void TestCorrectDuplicateRetrieval()
        {
            string json = "{ \"this_prop\": { \"to_retrieve\": notthisone }, \"to_retrieve\": thisone}";

            string expected = "thisone";
            string value;
            TwitterObject obj = new TwitterObject(json);
            Assert.IsTrue(obj.TryGetProperty("to_retrieve", out value));
            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void TestRetrievalOfNestedProps()
        {
            string json = "{ \"first_prop\": {\"prop\": getthisproperty, \"otherprop\": not }, \"other_prop\": none}";

            string expected = "getthisproperty";
            string actual = "";
            string expectedInnerJson = "\"prop\": getthisproperty, \"otherprop\": not ";
            string actualInnerJson = "";

            TwitterObject obj = new TwitterObject(json);
            
            Assert.IsTrue(obj.TryGetProperty("first_prop", out actualInnerJson));
            Assert.AreEqual(expectedInnerJson, actualInnerJson);

            TwitterObject inner = new TwitterObject(actualInnerJson);

            Assert.IsTrue(inner.TryGetProperty("prop", out actual));
            Assert.AreEqual(expected, actual);
        }
    }
}
