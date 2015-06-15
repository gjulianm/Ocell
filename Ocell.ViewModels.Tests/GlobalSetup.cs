using AsyncOAuth;
using NUnit.Framework;
using Ocell.Library;
using Ocell.Library.Twitter;
using System.Security.Cryptography;

namespace Ocell.ViewModels.Tests
{
    [SetUpFixture]
    public class GlobalSetup
    {
        [SetUp]
        public void Setup()
        {
            OAuthUtility.ComputeHash = (key, buffer) => { using (var hmac = new HMACSHA1(key)) { return hmac.ComputeHash(buffer); } };

            ServiceDispatcher.ApplicationKey = SensitiveData.TestConsumerToken;
            ServiceDispatcher.ApplicationSecret = SensitiveData.TestConsumerSecret;
        }
    }
}
