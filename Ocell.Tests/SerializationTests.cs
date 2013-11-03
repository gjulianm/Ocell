using AsyncOAuth;
using NUnit.Framework;
using Ocell.Library;
using Polenter.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using TweetSharp;

namespace Ocell.Tests
{
    [TestFixture]
    public class SerializationTests
    {
        public static IEnumerable<TwitterStatus> Statuses { get; set; }

        [TestFixtureSetUp]
        public static void Setup()
        {
            OAuthUtility.ComputeHash = (key, buffer) => { using (var hmac = new HMACSHA1(key)) { return hmac.ComputeHash(buffer); } };
            var client = new TwitterService(SensitiveData.TestConsumerToken, SensitiveData.TestConsumerSecret, SensitiveData.TestAccessToken, SensitiveData.TestAccessSecret);
            var task = client.ListTweetsOnHomeTimelineAsync(new ListTweetsOnHomeTimelineOptions { Count = 100 });
            task.Wait();
            Statuses = task.Result.Content ?? new List<TwitterStatus>();
            Debug.WriteLine("Received {0} statuses.", Statuses.Count());
        }

        [Test]
        public void Serialization()
        {
            var serializer = new SharpSerializer(new SharpSerializerBinarySettings { Mode = BinarySerializationMode.Burst });

            using (var file = File.Create("serialization"))
                serializer.Serialize(Statuses, file);
        }

        [Test]
        public void Deserialization()
        {
            var serializer = new SharpSerializer(new SharpSerializerBinarySettings { Mode = BinarySerializationMode.Burst });
            IEnumerable<TwitterStatus> deserializedStatuses = null;

            using (var file = File.Create("serialization"))
            {
                serializer.Serialize(Statuses.ToList(), file);
                file.Seek(0, SeekOrigin.Begin);
                deserializedStatuses = serializer.Deserialize(file) as IEnumerable<TwitterStatus>;
            }

            Assert.NotNull(deserializedStatuses);
            CollectionAssert.AreEquivalent(Statuses, deserializedStatuses);
        }

        [TestCaseSource("Serializers")]
        public void TestSerializationTimes(SharpSerializer serializer)
        {
            var stopwatch = new Stopwatch();
            using (var file = File.Create("serialization"))
            {
                stopwatch.Start();
                serializer.Serialize(Statuses, file);
                stopwatch.Stop();
                Assert.Pass("Time: {0} ms", stopwatch.ElapsedMilliseconds);
            }
        }

        [TestCaseSource("Deserializers")]
        public void TestDeserializationTimes(SharpSerializer serializer)
        {
            var stopwatch = new Stopwatch();
            using (var file = File.Create("serialization"))
            {
                serializer.Serialize(Statuses, file);
                file.Seek(0, SeekOrigin.Begin);
                stopwatch.Start();
                var deserializedStatuses = serializer.Deserialize(file) as IEnumerable<TwitterStatus>;
                stopwatch.Stop();
                Assert.Pass("Time: {0} ms", stopwatch.ElapsedMilliseconds);
            }
        }

        public IEnumerable<TestCaseData> Deserializers
        {
            get { return InternalSerializers.Select(x => x.SetName("Perf.: Deserialize - " + x.TestName)); }
        }

        public IEnumerable<TestCaseData> Serializers
        {
            get { return InternalSerializers.Select(x => x.SetName("Perf.: Serialize - " + x.TestName)); }
        }

        public IEnumerable<TestCaseData> InternalSerializers
        {
            get
            {
                yield return new TestCaseData(new SharpSerializer(false))
                    .SetName("No binary serialization");
                yield return new TestCaseData(new SharpSerializer(new SharpSerializerBinarySettings { Mode = BinarySerializationMode.Burst }))
                    .SetName("Binary - Burst");
                yield return new TestCaseData(new SharpSerializer(new SharpSerializerBinarySettings { Mode = BinarySerializationMode.SizeOptimized }))
                   .SetName("Binary - Size");
                yield return new TestCaseData(new SharpSerializer(new SharpSerializerBinarySettings
                {
                    Mode = BinarySerializationMode.Burst,
                    IncludeAssemblyVersionInTypeName = false,
                    IncludeCultureInTypeName = false,
                    IncludePublicKeyTokenInTypeName = false
                }))
                   .SetName("Binary - Burst - Noincludes");
                yield return new TestCaseData(new SharpSerializer(new SharpSerializerBinarySettings
                {
                    Mode = BinarySerializationMode.Burst,
                    IncludeAssemblyVersionInTypeName = false,
                    IncludeCultureInTypeName = false,
                    IncludePublicKeyTokenInTypeName = false
                }))
                   .SetName("Binary - Size - Noincludes");
            }
        }
    }
}