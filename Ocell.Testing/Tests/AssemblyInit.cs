using System;
using DanielVaughan;
using DanielVaughan.InversionOfControl;
using DanielVaughan.Net;
using DanielVaughan.Services;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocell.Pages.Search;
using DanielVaughan.InversionOfControl.Containers.SimpleContainer;
using DanielVaughan.Services.Implementation;

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
        }
    }
}
