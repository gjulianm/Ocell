using NUnit.Framework;
using Ocell.Library.Filtering;
using System;
using System.Linq;

namespace Ocell.Tests.Filtering
{
    [TestFixture]
    public class ElementFilterTests
    {
        private class ElementFilterDummyImpl : ElementFilter<string>
        {
            public override string Selector(string item)
            {
                return item;
            }
        }

        [Test]
        public void ExcludeElement_ExcludeOnMatchAndFilterExpired_ReturnsFalse()
        {
            var filter = new ElementFilterDummyImpl
            {
                Filter = "test",
                IsValidUntil = DateTime.Now.AddYears(-1),
                MatchMode = MatchMode.Contains,
                Mode = ExcludeMode.ExcludeOnMatch
            };

            Assert.IsFalse(filter.ExcludeElement("test string"));
            Assert.IsFalse(filter.ExcludeElement("no match"));
        }

        [Test]
        public void ExcludeElement_ExcludeOnNoMatchAndFilterExpired_ReturnsFalse()
        {
            var filter = new ElementFilterDummyImpl
            {
                Filter = "test",
                IsValidUntil = DateTime.Now.AddYears(-1),
                MatchMode = MatchMode.Contains,
                Mode = ExcludeMode.ExcludeOnNoMatch
            };

            Assert.IsFalse(filter.ExcludeElement("test string"));
            Assert.IsFalse(filter.ExcludeElement("no match"));
        }

        [Test]
        public void ExcludeElement_ExcludeOnMatchAndMatches_ElementExcluded()
        {
            var filter = new ElementFilterDummyImpl
            {
                Filter = "test",
                IsValidUntil = DateTime.Now.AddYears(1),
                MatchMode = MatchMode.Contains,
                Mode = ExcludeMode.ExcludeOnMatch
            };

            Assert.IsTrue(filter.ExcludeElement("test string"));
        }

        [Test]
        public void ExcludeElement_ExcludeOnMatchAndDoesntMatch_ElementNotExcluded()
        {
            var filter = new ElementFilterDummyImpl
            {
                Filter = "test",
                IsValidUntil = DateTime.Now.AddYears(1),
                MatchMode = MatchMode.Contains,
                Mode = ExcludeMode.ExcludeOnMatch
            };

            Assert.IsFalse(filter.ExcludeElement("no string"));
        }

        [Test]
        public void ExcludeElement_ExcludeOnNoMatchAndMatches_ElementNotExcluded()
        {
            var filter = new ElementFilterDummyImpl
            {
                Filter = "test",
                IsValidUntil = DateTime.Now.AddYears(1),
                MatchMode = MatchMode.Contains,
                Mode = ExcludeMode.ExcludeOnNoMatch
            };

            Assert.IsFalse(filter.ExcludeElement("test string"));
        }

        [Test]
        public void ExcludeElement_ExcludeOnNoMatchAndDoesntMatch_ElementExcluded()
        {
            var filter = new ElementFilterDummyImpl
            {
                Filter = "test",
                IsValidUntil = DateTime.Now.AddYears(1),
                MatchMode = MatchMode.Contains,
                Mode = ExcludeMode.ExcludeOnNoMatch
            };

            Assert.IsTrue(filter.ExcludeElement("bi string"));
        }

        [Test]
        public void ListExcludesElement_ElementDoesntMatchAny_NotExcluded()
        {
            var filterStrings = new string[] { "a", "test", "for", "things" };
            var filters = filterStrings.Select(s => new ElementFilterDummyImpl
            {
                Filter = s,
                IsValidUntil = DateTime.Now.AddYears(1),
                MatchMode = MatchMode.Contains,
                Mode = ExcludeMode.ExcludeOnMatch
            });

            Assert.IsFalse(filters.ExcludesElement("notpresent"));
        }

        [Test]
        public void ListExcludesElement_ElementMatches_Excluded()
        {
            var filterStrings = new string[] { "a", "test", "for", "things" };
            var filters = filterStrings.Select(s => new ElementFilterDummyImpl
            {
                Filter = s,
                IsValidUntil = DateTime.Now.AddYears(1),
                MatchMode = MatchMode.Contains,
                Mode = ExcludeMode.ExcludeOnMatch
            });

            Assert.IsTrue(filters.ExcludesElement("a test"));
        }
    }
}
