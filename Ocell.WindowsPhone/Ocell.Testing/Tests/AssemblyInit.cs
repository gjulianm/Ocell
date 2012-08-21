using DanielVaughan;
using DanielVaughan.InversionOfControl;
using DanielVaughan.InversionOfControl.Containers.SimpleContainer;
using DanielVaughan.Services;
using DanielVaughan.Services.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocell.Library.Twitter;

namespace Ocell.Testing.Tests
{
    public class AssemblyInit
    {
        [AssemblyInitialize]
        public static void Initialize()
        {
            SimpleContainer container = new SimpleContainer();
            container.InitializeServiceLocator();

            Dependency.Register<INavigationService, MockNavigationService>(true);
            Dependency.Register<IMessageService, MockMessageService>(true);
            Dependency.Register<IMarketplaceService, MockMarketplaceService>(true);
            Dependency.Register<IUserProvider, MockUserProvider>();
        }
    }
}
