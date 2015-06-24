using System.Diagnostics;
using Ocell.Library.Twitter;

namespace Ocell.Library.RuntimeData
{
    public class UserProviderDispatcher : BaseDispatcher<UserProvider>
    {
        protected override UserProvider CreateNewItem(UserToken token)
        {
            var provider = new UserProvider { User = token };
            provider.Start();

#if DEBUG
            provider.Error +=
                (sender, response) => Debug.WriteLine("UserProvider {0} error: {1}", token, response.Error);

            provider.Finished +=
                (sender, args) =>
                    Debug.WriteLine("UserProvider {0} finished with {1} users.", token, provider.Users.Count);
#endif

            return provider;
        }
    }
}
