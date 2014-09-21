using NUnit.Framework;
using System;
using System.Windows.Input;

namespace Ocell.Tests.TestHelpers.Events
{
    public static class CanExecuteChangedExpectation
    {
        public static void ShouldChangeCanExecuteWhen<T>(this T owner, Action when)
            where T : ICommand
        {
            bool eventRaised = false;
            owner.CanExecuteChanged += (sender, e) => eventRaised = true;

            when();

            Assert.IsTrue(eventRaised, "CanExecuteChanged not fired.");
        }
    }
}
