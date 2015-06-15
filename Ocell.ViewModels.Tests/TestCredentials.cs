using Ocell.Library;
using Ocell.Library.Twitter;

namespace Ocell.ViewModels.Tests
{
    public static class TestCredentials
    {
        public static UserToken GetTwitterUser()
        {
            return new UserToken
            {
                Key = SensitiveData.TestAccessToken,
                Secret = SensitiveData.TestAccessSecret,
                ScreenName = "ocell_test"
            };
        }
    }
}
