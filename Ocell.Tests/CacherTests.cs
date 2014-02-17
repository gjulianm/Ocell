using AncoraMVVM.Base.Collections;
using AncoraMVVM.Base.IoC;
using NUnit.Framework;
using Ocell.Library.Twitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TweetSharp;

namespace Ocell.Tests
{
    [TestFixture]
    public class CacherTests
    {
        static Random rand = new Random((int)DateTime.Now.Ticks);
        public static TwitterStatus RandomTwitterStatus()
        {
            var bytes = new byte[50];
            rand.NextBytes(bytes);
            var status = new TwitterStatus
            {
                Id = rand.Next(),
                CreatedDate = DateTime.Now - TimeSpan.FromMinutes(rand.Next(30, 200)),
                Text = Convert.ToBase64String(bytes),
                User = new TwitterUser
                {
                    Name = Convert.ToBase64String(bytes.Select(x => (byte)(x + rand.Next())).ToArray()),
                    ScreenName = "testaaa"
                },
                Entities = new TwitterEntities()
            };

            status.CreatedDate = new DateTime(status.CreatedDate.Ticks, DateTimeKind.Unspecified);

            return status;
        }

        public static IEnumerable<TwitterStatus> GetRandomList(int size)
        {
            return Enumerable.Range(0, size).Select(x => RandomTwitterStatus());
        }

        [TestFixtureSetUp]
        public static void Setup()
        {
            Dependency.Provider = new MockProvider();
        }

        [TestFixtureTearDown]
        public static void Teardown()
        {
            Dependency.Provider = null;
        }

        [Test]
        public void CacheSave_CollectionDoesNotVary()
        {
            var statuses = GetRandomList(20).ToList();
            var resource = new TwitterResource { Type = ResourceType.Home, User = new UserToken { ScreenName = "ocell_test" } };

            Cacher.SaveToCache(resource, statuses);

            var received = Cacher.GetFromCache(resource);

            foreach(var pair in statuses.Zip(received))
                AssertEx.PropertyValuesAreEquals(pair.Item2, pair.Item1);
        }
    }
}
