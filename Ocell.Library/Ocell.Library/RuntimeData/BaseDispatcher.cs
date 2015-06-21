using System.Collections.Generic;
using System.Linq;
using Ocell.Library.Twitter;

namespace Ocell.Library.RuntimeData
{
    public abstract class BaseDispatcher<T>
    {
        private Dictionary<UserToken, T> items = new Dictionary<UserToken, T>();
        private object lockFlag = new object();

        protected T CreateAndSaveItem(UserToken token)
        {
            T item = CreateNewItem(token);

            lock (lockFlag)
            {
                items[token] = item;
            }

            return item;
        }

        public virtual T GetForUser(UserToken token)
        {
            if (token == null || token.Key == null)
                throw new ApplicationException("Can't get a service for a null token.");

            lock (lockFlag)
            {
                T item;

                if (items.TryGetValue(token, out item))
                    return item;

                return CreateAndSaveItem(token);
            }
        }

        public virtual T GetDefault()
        {
            return GetForUser(Config.Accounts.Value.FirstOrDefault());
        }

        public void CreateForAllAccounts()
        {
            foreach (var account in Config.Accounts.Value)
                CreateAndSaveItem(account);
        }

        protected abstract T CreateNewItem(UserToken token);
    }
}
