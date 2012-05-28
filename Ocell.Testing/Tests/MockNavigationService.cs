using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

using DanielVaughan.Services;

namespace Ocell.Testing.Tests
{
    public class MockNavigationService : INavigationService
    {
        protected Stack<Uri> _pages = new Stack<Uri>();

        public Uri Source
        {
            get
            {
                return _pages.Peek();
            }
        }

        public void GoBack()
        {
            _pages.Pop();
        }

        public void Navigate(string relativeUrl)
        {
            _pages.Push(new Uri(relativeUrl, UriKind.Relative));
        }

        public void Navigate(Uri source)
        {
            _pages.Push(source);
        }
    }
}
