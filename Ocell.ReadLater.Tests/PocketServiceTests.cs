using AncoraMVVM.Rest;
using NUnit.Framework;
using Ocell.Library.ReadLater.Pocket;
using System;
using System.Threading.Tasks;

namespace Ocell.ReadLater.Tests
{
    [TestFixture]
    public class PocketServiceTests
    {
        public PocketService Service
        {
            get
            {
                return new PocketService(TestAccounts.PocketUser, TestAccounts.PocketPassword);
            }
        }

        private async Task TestEndpoint(Func<Task<HttpResponse>> task)
        {
            var response = await task();

            Assert.IsTrue(response.Succeeded, "Request not suceeded. Error code {0}, inner exception {1}, response {2}", response.StatusCode, response.InnerException, response.StringContents); ;
        }

        [Test]
        public async Task CheckCredentials()
        {
            await TestEndpoint(() => Service.CheckCredentials());
        }

        [Test]
        public async Task AddUrl_OneParam()
        {
            await TestEndpoint(() => Service.AddUrl("http://github.com"));
        }

        [Test]
        public async Task AddUrl_Tweet()
        {
            await TestEndpoint(() => Service.AddUrl("http://github.com", 66565656));
        }
    }
}
