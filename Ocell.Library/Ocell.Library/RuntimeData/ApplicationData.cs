using System.Linq;
using Ocell.Library.Twitter;

namespace Ocell.Library.RuntimeData
{
    public static class ApplicationData
    {
        static UserToken account;
        public static UserToken CurrentAccount
        {
            get
            {
                if (account == null)
                    return Config.Accounts.Value.FirstOrDefault();
                return account;
            }
            set
            {
                account = value;
            }
        }

        public static UserProviderDispatcher UserProviders { get; private set; }

        public static void InitializeRuntimeData()
        {
            UserProviders = new UserProviderDispatcher();
            UserProviders.CreateForAllAccounts();
        }
    }
}
