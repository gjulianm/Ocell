using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Threading;
using Ocell.Library.Tasks;
using Ocell.Library.Twitter;
#if !BACKGROUND_AGENT
using Ocell.Library.Filtering;
#endif


namespace Ocell.Library
{
    public static class TypeExtensions
    {
        public static bool HasParameterlessConstructor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }
    }

    public static partial class Config
    {
        private static Mutex _mutex = new Mutex(false, "Ocell.IsolatedStorageSettings_MUTEX");
        private const int MutexTimeout = 1000;

        private static T CreateDefault<T>()
        {
            var type = typeof(T);

            if (type.HasParameterlessConstructor())
                return Activator.CreateInstance<T>();
            else
                return default(T);
        }

        private static T GenericGetFromConfig<T>(string key, ref T element)
        {
            if (element != null)
                return element;

            if (_mutex.WaitOne(MutexTimeout))
            {
                IsolatedStorageSettings config = IsolatedStorageSettings.ApplicationSettings;

                try
                {
                    if (!config.TryGetValue<T>(key, out element))
                    {
                        element = CreateDefault<T>();

                        if (DefaultValues.ContainsKey(key))
                            element = (T)DefaultValues[key];

                        config.Add(key, element);
                        config.Save();
                    }
                }
                catch (InvalidCastException)
                {
                    element = CreateDefault<T>();
                    config.Remove(key);
                    config.Save();
                }
                catch (Exception)
                {
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            }

            if (element == null)
                element = CreateDefault<T>();

            return element;
        }

        private static void GenericSaveToConfig<T>(string Key, ref T element, T value)
        {
            if (value == null)
                return;

            if (_mutex.WaitOne(MutexTimeout))
            {
                IsolatedStorageSettings conf = IsolatedStorageSettings.ApplicationSettings;

                try
                {
                    element = value;
                    if (conf.Contains(Key))
                        conf[Key] = value;
                    else
                        conf.Add(Key, value);
                    conf.Save();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            }
        }

        public static void ClearAll()
        {
            if (_mutex.WaitOne(MutexTimeout))
            {
                try
                {
                    IsolatedStorageSettings.ApplicationSettings.Clear();
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            }
        }

        static Dictionary<string, object> defaultValues;
        static Dictionary<string, object> DefaultValues
        {
            get
            {
                if (defaultValues == null)
                    GenerateDefaultDictionary();
                return defaultValues;
            }
        }

        const string pushEnabledKey = "PUSHENABLED";
        static bool? pushEnabled;
        public static bool? PushEnabled
        {
            get
            {
#if OCELL_FULL
                return GenericGetFromConfig(pushEnabledKey, ref pushEnabled);
#else
                return false;
#endif
            }
            set
            {
#if OCELL_FULL
                GenericSaveToConfig(pushEnabledKey, ref pushEnabled, value);
#endif
            }
        }
    }
}
