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
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Phone.Tasks;
using System.Diagnostics;
using Microsoft.Phone.Info;
using Ocell.Localization;
using System.Threading;

namespace Ocell.Library
{
    /// <summary>
    /// This class was made by Andy Pennel https://blogs.msdn.com/b/andypennell/archive/2010/11/01/error-reporting-on-windows-phone-7.aspx?Redirected=true
    /// </summary>

    public class LittleWatson
    {
        const string filename = "LittleWatson.txt";

        public static void ReportException(Exception ex, string extra)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    SafeDeleteFile(store);

                    using (TextWriter output = new StreamWriter(store.CreateFile(filename)))
                    {
                        output.WriteLine("Phone: {0} by {1}", DeviceStatus.DeviceName, DeviceStatus.DeviceManufacturer);
                        output.WriteLine("Memory usage: {0} KB", (DeviceStatus.ApplicationCurrentMemoryUsage / (1024)).ToString());
                        output.WriteLine("Memory peak: {0} KB", (DeviceStatus.ApplicationPeakMemoryUsage / (1024)).ToString());
                        output.WriteLine("Firmware version: {0}", DeviceStatus.DeviceFirmwareVersion);
                        output.WriteLine("Assembly name: {0}", System.Reflection.Assembly.GetCallingAssembly().FullName);
                        output.WriteLine("Language: {0}", Thread.CurrentThread.CurrentCulture.Name);
                        output.WriteLine("State: {0}", TrialInformation.State);
                        output.WriteLine(extra);
                        output.WriteLine(ex.GetType().FullName);
                        output.WriteLine(ex.Message);
                        output.WriteLine(ex.StackTrace);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void CheckForPreviousException()
        {
            try
            {
                string contents = null;
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(filename))
                    {
                        using (TextReader reader = new StreamReader(store.OpenFile(filename, FileMode.Open, FileAccess.Read, FileShare.None)))
                        {
                            contents = reader.ReadToEnd();
                        }
                        SafeDeleteFile(store);
                    }
                }
                if (contents != null)
                {
                    if (contents.Length >= 50000)
                        contents = contents.Substring(0, 50000); // just in case.

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (MessageBox.Show(Resources.ErrorReportMessage, Resources.ErrorReport, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            EmailComposeTask email = new EmailComposeTask();
                            email.To = "gjulian93@gmail.com";
                            email.Subject = "Ocell Error Report";
                            email.Body = contents;
                            SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication()); // line added 1/15/2011
                            email.Show();
                        }
                    });
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication());
            }
        }

        private static void SafeDeleteFile(IsolatedStorageFile store)
        {
            try
            {
                store.DeleteFile(filename);
            }
            catch (Exception)
            {
            }
        }
    }
}
