using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ocell.Testing
{
    public class PerformanceMeter
    {
        Action _method1;
        Action _method2;

        public PerformanceMeter(Action method1, Action method2)
        {
            _method1 = method1;
            _method2 = method2;
        }

        private long MeasureRunningTime(Action method)
        {
            var cron = new Stopwatch();
            cron.Start();
            method();
            cron.Stop();
            return cron.ElapsedMilliseconds;
        }

        public string MeasureAverageRunningTime(uint resolution)
        {
            int numExecutions = (int)Math.Pow(10, resolution / 2);

            long totalTime1 = 0;
            long totalTime2 = 0;

            for (int i = 0; i < numExecutions; i++)
            {
                totalTime1 += MeasureRunningTime(_method1);
                totalTime2 += MeasureRunningTime(_method2);
            }

            long avgTime1 = totalTime1 / numExecutions;
            long avgTime2 = totalTime2 / numExecutions;

            string report = String.Format("Measuring average performance of methods 1 ({0}) and 2({1}):\n" +
                "Method 1 average time: {2} ms\n" +
                "Method 2 average time: {3} ms\n" +
                "Performance difference: {4} ms", _method1.Method.Name, _method2.Method.Name,
                avgTime1, avgTime2, avgTime1 - avgTime2);

            return report;
        }
    }

    public class EnumerableMethodPerfMeter<T>
    {
        Action<IEnumerable<T>> _method1;
        Action<IEnumerable<T>> _method2;

        public EnumerableMethodPerfMeter(Action<IEnumerable<T>> method1, Action<IEnumerable<T>> method2,
            Func<T> randomGenerator)
        {
            _method1 = method1;
            _method2 = method2;
            RandomGenerator = randomGenerator;
        }

        public Func<T> RandomGenerator { get; set; }

        public IEnumerable<T> GenerateRandomCollectionOfSize(long size)
        {
            List<T> collection = new List<T>();

            for (int i = 0; i < size; i++)
                collection.Add(RandomGenerator());

            return collection;
        }

        private long MeasureRunningTime(Action<IEnumerable<T>> method, IEnumerable<T> collection)
        {
            var cron = new Stopwatch();
            cron.Start();
            method(collection);
            cron.Stop();
            return cron.ElapsedMilliseconds;
        }

        private double MeasureAverageRunningTime(Action<IEnumerable<T>> method, IEnumerable<T> collection, int resolution)
        {
            int numExecutions = (int)Math.Pow(10, resolution / 2);

            double total = 0;

            for (int i = 0; i < numExecutions; i++)
                total += MeasureRunningTime(method, collection);

            return total / numExecutions;
        }
        private IEnumerable<long> GetCollectionSizesForResolution(int resolution)
        {
            // Get sizes in a logarithmic scale. Base 10.
            const long defaultEndValue = 100;
            long startValue, endValue;

            startValue = 1;
            endValue = defaultEndValue * (long)Math.Pow(10, (int)Math.Log10(resolution));

            int defaultSteps = 5;
            long steps = defaultSteps * resolution;

            double baseStart, baseEnd;

            baseStart = Math.Log10(startValue);
            baseEnd = Math.Log10(endValue);

            double step = (baseEnd - baseStart) / steps;

            for (double value = baseStart; value <= baseEnd; value += step)
                yield return (long)Math.Pow(10, value);
        }

        public string MeasureVariantSizePerformance(int resolution = 2)
        {
            string report = "";

            var sizes = GetCollectionSizesForResolution(resolution).ToList();
            double diffSum = 0;

            int quarterNum = 0;
            var quarters = new List<double> {0, 0, 0, 0};
            var quarterSize = new List<int> { 0, 0, 0, 0 };
            int thisQuarterSize = 1;

            report += String.Format("Measuring performance for method 1 ({0}) and method 2 ({1}).\n\n",
                _method1.Method.Name, _method2.Method.Name);

            for (int i = 0; i < sizes.Count(); i++, thisQuarterSize++)
            {
                var size = sizes.ElementAt(i);
                var collection = GenerateRandomCollectionOfSize(size);

                double runningTime1 = MeasureAverageRunningTime(_method1, collection, resolution);
                double runningTime2 = MeasureAverageRunningTime(_method2, collection, resolution);

                double diff = runningTime1 - runningTime2;

                if (i >= ((double)sizes.Count() / 4) * (quarterNum + 1))
                {
                    quarterSize[quarterNum] = thisQuarterSize;
                    thisQuarterSize = 1;
                    quarterNum++;
                }

                quarters[quarterNum] = quarters[quarterNum] + diff;
                diffSum += diff;

                report += String.Format("Partial report {0}, collection size {1}:\n" +
                    "\tMethod 1 running time: {2} ms\n" +
                    "\tMethod 2 running time: {3} ms\n" +
                    "\tDifference: {4} ms\n\n", i, size, runningTime1, runningTime2, diff);
            }

            quarters = quarters.Select((s, i) => s / (quarterSize[i] == 0 ? 1 : quarterSize[i])).ToList();

            int faster = diffSum < 0 ? 1 : 2;

            report += String.Format("Final average difference: {0}\n" +
                "Faster method: {5}\n" +
                "Average difference in quarters:\n" +
                "\tQ1: {1} ms\n" +
                "\tQ2: {2} ms\n" +
                "\tQ3: {3} ms\n" +
                "\tQ4: {4} ms\n", diffSum / sizes.Count(), quarters[0],
                quarters[1], quarters[2], quarters[3], faster);

            return report;
        }
    }
}