using Ocell.Library.Twitter;

namespace Ocell.Library.RuntimeData
{
    public class UserProviderDispatcher : BaseDispatcher<UserProvider>
    {
        protected override UserProvider CreateNewItem(UserToken token)
        {
            var provider = new UserProvider { User = token };
            provider.Start();

            return provider;
        }
    }
}
