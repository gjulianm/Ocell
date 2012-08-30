using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ocell.Library.Collections;
using System.Collections.Specialized;

namespace Library.Tests
{
    [TestClass]
    public class FilteredSortedObservableTests
    {
        const int DefaultSize = 50;

        IEnumerable<int> GenerateList(int count)
        {
            Random rand = new Random();
            while (count > 0)
            {
                yield return rand.Next();
                count--;
            }
        }

        [TestMethod]
        public void TestSort()
        {
            var sorted = new FilteredSortedObservable<int>(GenerateList(DefaultSize));
            
            for (int i = 1; i < sorted.Count; i++)
                Assert.IsTrue(sorted[i] > sorted[i - 1]);
        }

        [TestMethod]
        public void TestEnumerable()
        {
            var sorted = new FilteredSortedObservable<int>(GenerateList(DefaultSize));

            int index = 0;

            using (var enumerator = sorted.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Assert.AreEqual(sorted[index], enumerator.Current);
                    index++;
                }
            }

            Assert.AreEqual(sorted.Count, index);
        }

        [TestMethod]
        public void TestRandomAccess()
        {
            var list = GenerateList(DefaultSize).ToList();
            var sorted = new FilteredSortedObservable<int>(list);

            Assert.AreEqual(sorted[0], list.Min(x => x));
        }

        [TestMethod]
        public void TestCollectionChangedEvents()
        {
            bool isRaised = false;
            Action<object, NotifyCollectionChangedEventArgs> action = (x, y) => isRaised = true;
            var sorted = new FilteredSortedObservable<int>(GenerateList(DefaultSize));

            sorted.CollectionChanged += (x, y) => isRaised = true;

            sorted.Add(3);
            Assert.IsTrue(isRaised);

            isRaised = false;

            sorted.Remove(3);
            Assert.IsTrue(isRaised);
        }

        [TestMethod]
        public void TestFilterCount()
        {
            var list = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            var sorted = new FilteredSortedObservable<int>(list);

            Assert.AreEqual(list.Count, sorted.Count);
            
            sorted.Filter = x => x > 3;

            Assert.AreEqual(4, sorted.Count);
        }

        [TestMethod]
        public void TestFilter()
        {
            var sorted = new FilteredSortedObservable<int>(GenerateList(DefaultSize));
            sorted.Filter = x => x % 2 == 0;

            foreach (var item in sorted)
                Assert.IsTrue(sorted.Filter.Invoke(item));
        }
    }
}
