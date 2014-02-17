using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ocell.Tests
{
    public static class AssertEx
    {
        public static void PropertyValuesAreEquals(object actual, object expected)
        {
            PropertyInfo[] properties = expected.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object expectedValue = property.GetValue(expected, null);
                object actualValue = property.GetValue(actual, null);

                if (actualValue is IEnumerable)
                {
                    if (actualValue == null)
                        Assert.AreEqual(expectedValue, actualValue);
                    else
                        CollectionAssert.AreEqual((IEnumerable)actualValue, (IEnumerable)expectedValue);
                }
                else if (!Equals(expectedValue, actualValue))
                    Assert.Fail("Property {0}.{1} does not match. Expected: {2} but was: {3}", property.DeclaringType.Name, property.Name, expectedValue, actualValue);
            }
        }

    }
}
