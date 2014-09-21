using NUnit.Framework;
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Ocell.Tests.TestHelpers.Events
{
    public class NotifyExpectation<T>
     where T : INotifyPropertyChanged
    {
        private readonly T owner;
        private readonly string propertyName;
        private readonly bool eventExpected;

        public NotifyExpectation(T owner,
            string propertyName, bool eventExpected)
        {
            this.owner = owner;
            this.propertyName = propertyName;
            this.eventExpected = eventExpected;
        }

        public void When(Action<T> action)
        {
            bool eventWasRaised = false;
            this.owner.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == this.propertyName)
                {
                    eventWasRaised = true;
                }
            };
            action(this.owner);

            Assert.AreEqual(this.eventExpected,
                eventWasRaised,
                "PropertyChanged on {0}", this.propertyName);
        }
    }

    public static class NotifyPropertyChanged
    {
        public static NotifyExpectation<T>
            ShouldNotifyOn<T, TProperty>(this T owner,
            Expression<Func<T, TProperty>> propertyPicker)
            where T : INotifyPropertyChanged
        {
            return NotifyPropertyChanged.CreateExpectation(owner,
                propertyPicker, true);
        }

        public static NotifyExpectation<T>
            ShouldNotNotifyOn<T, TProperty>(this T owner,
            Expression<Func<T, TProperty>> propertyPicker)
            where T : INotifyPropertyChanged
        {
            return NotifyPropertyChanged.CreateExpectation(owner,
                propertyPicker, false);
        }

        private static NotifyExpectation<T>
            CreateExpectation<T, TProperty>(T owner,
            Expression<Func<T, TProperty>> pickProperty,
            bool eventExpected) where T : INotifyPropertyChanged
        {
            string propertyName =
                ((MemberExpression)pickProperty.Body).Member.Name;
            return new NotifyExpectation<T>(owner,
                propertyName, eventExpected);
        }
    }
}
