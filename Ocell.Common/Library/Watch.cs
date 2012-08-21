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
using System.Diagnostics;

namespace Ocell.Library
{
    public static class TimeTracker
    {
        public static void Track(Action function)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            function();
            watch.Stop();
            Debug.WriteLine("Method " + function.Method.Name +" took " + watch.ElapsedMilliseconds + " ms.");
        }
    }
}
