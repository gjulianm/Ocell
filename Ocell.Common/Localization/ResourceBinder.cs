using System;


namespace Ocell.Localization
{
    public class LocalizedResources
    {
        private static readonly Resources localizedResources = new Resources();

        public Resources Strings
        {
            get { return localizedResources; }
        }
    }
}
