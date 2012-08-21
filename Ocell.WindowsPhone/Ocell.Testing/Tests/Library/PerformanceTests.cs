using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ocell.Library.Twitter;
using Ocell.Library.Twitter.Comparers;
using TweetSharp;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocell.Library;
using DanielVaughan;
using DanielVaughan.Services;
using DanielVaughan.Services.Implementation;
using System.Threading;

namespace Ocell.Testing
{
    [TestClass]
    [Tag("Performance")]
    public class PerformanceTests
    {
        List<TwitterStatus> GenerateRandomCollectionOfSize(long size, Func<TwitterStatus> RandomGenerator)
        {
            List<TwitterStatus> collection = new List<TwitterStatus>();

            for (int i = 0; i < size; i++)
                collection.Add(RandomGenerator());

            return collection;
        }

        [TestMethod]
        public void TestUnionVsAddWithComparers()
        {
            Func<TwitterStatus> randomGen = () => new TwitterStatus { Id = (new Random()).Next(0, 9000000) };
            Action<IEnumerable<TwitterStatus>> unionMethod = collection =>
            {
                var source = GenerateRandomCollectionOfSize(100, randomGen);
                source.Union(collection, new TwitterStatusEqualityComparer()).ToList();
            };

            Action<IEnumerable<TwitterStatus>> addMethod = collection =>
            {
                var source = GenerateRandomCollectionOfSize(100, randomGen);
                var comparer = new TwitterStatusEqualityComparer();
                foreach (var element in collection)
                    if (!source.Contains(element, comparer))
                        source.Add(element);
            };

            var meter = new EnumerableMethodPerfMeter<TwitterStatus>(unionMethod, addMethod, randomGen);

            string report = meter.MeasureVariantSizePerformance(4);

            Assert.AreEqual(0, 0, report);
            Debug.WriteLine(report);
        }

        [TestMethod]
        public void TestUnionVsAddWithoutComparers()
        {
            Func<TwitterStatus> randomGen = () => new TwitterStatus { Id = (new Random()).Next(0, 9000000) };
            Action<IEnumerable<TwitterStatus>> unionMethod = collection =>
            {
                var source = GenerateRandomCollectionOfSize(100, randomGen);
                source.Union(collection).ToList();
            };

            Action<IEnumerable<TwitterStatus>> addMethod = collection =>
            {
                var source = GenerateRandomCollectionOfSize(100, randomGen);
                foreach (var element in collection)
                    if (!source.Contains(element))
                        source.Add(element);
            };

            var meter = new EnumerableMethodPerfMeter<TwitterStatus>(unionMethod, addMethod, randomGen);

            string report = meter.MeasureVariantSizePerformance(4);
            Assert.AreEqual(0, 0, report);
            Debug.WriteLine(report);
        }

        [TestMethod]
        public void TestUnionWithAndWithoutComparers()
        {
            Func<TwitterStatus> randomGen = () => new TwitterStatus { Id = (new Random()).Next(0, 9000000) };
            Action<IEnumerable<TwitterStatus>> withoutComparer = collection =>
            {
                var source = GenerateRandomCollectionOfSize(100, randomGen);
                source.Union(collection).ToList();
            };

            Action<IEnumerable<TwitterStatus>> withComparer = collection =>
            {
                var source = GenerateRandomCollectionOfSize(100, randomGen);
                source.Union(collection, new TwitterStatusEqualityComparer()).ToList();
            };

            var meter = new EnumerableMethodPerfMeter<TwitterStatus>(withComparer, withoutComparer, randomGen);

            string report = meter.MeasureVariantSizePerformance(4);
            Assert.AreEqual(0, 0, report);
            Debug.WriteLine(report);
        }
        [TestMethod]
        public void TestAddWithAndWithoutComparers()
        {
            Func<TwitterStatus> randomGen = () => new TwitterStatus { Id = (new Random()).Next(0, 9000000) };
            Action<IEnumerable<TwitterStatus>> withoutComparer = collection =>
            {
                var source = GenerateRandomCollectionOfSize(40, randomGen);
                foreach (var element in collection)
                    if (!source.Contains(element))
                        source.Add(element);
            };

            Action<IEnumerable<TwitterStatus>> withComparer = collection =>
            {
                var source = GenerateRandomCollectionOfSize(40, randomGen);
                var comparer = new TwitterStatusEqualityComparer();
                foreach (var element in collection)
                    if (!source.Contains(element, comparer))
                        source.Add(element);
            };

            var meter = new EnumerableMethodPerfMeter<TwitterStatus>(withComparer, withoutComparer, randomGen);

            string report = meter.MeasureVariantSizePerformance(4);
            Assert.AreEqual(0, 0, report);
            Debug.WriteLine(report);
        }

        [TestMethod]
        public void TestIntersectThenAdd()
        {
            Func<TwitterStatus> randomGen = () => new TwitterStatus { Id = (new Random()).Next(0, 9000000) };
            Action<IEnumerable<TwitterStatus>> intersect = collection =>
            {
                var source = GenerateRandomCollectionOfSize(40, randomGen);
                collection = collection.Except(source);
                foreach (var element in collection)
                    source.Add(element);
            };

            Action<IEnumerable<TwitterStatus>> addMethod = collection =>
            {
                var source = GenerateRandomCollectionOfSize(40, randomGen);

                foreach (var element in collection)
                    if (!source.Contains(element))
                        source.Add(element);
            };

            var meter = new EnumerableMethodPerfMeter<TwitterStatus>(addMethod, intersect, randomGen);

            string report = meter.MeasureVariantSizePerformance(4);
            Assert.AreEqual(0, 0, report);
            Debug.WriteLine(report);
        }
    }
}