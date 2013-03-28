using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Threading;
using Ocell.Library.Tasks;
using Ocell.Library.Twitter;
using Ocell.Library.Security;
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
        private static string mutexName = "IsolatedStorageSettings";
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

            T copy = default(T);

            MutexUtil.DoWork(mutexName, () =>
            {
                IsolatedStorageSettings config = IsolatedStorageSettings.ApplicationSettings;

                try
                {
                    if (!config.TryGetValue<T>(key, out copy))
                    {
                        if (DefaultValues.ContainsKey(key))
                            copy = (T)DefaultValues[key];
                        else
                            copy = CreateDefault<T>();

                        config.Add(key, copy);
                        config.Save();
                    }
                }
                catch (InvalidCastException)
                {
                    copy = CreateDefault<T>();
                    config.Remove(key);
                    config.Save();
                }
            });

            element = copy;

            if (element == null)
                element = CreateDefault<T>();

            return element;
        }

        private static void GenericSaveToConfig<T>(string Key, ref T element, T value)
        {
            if (value == null)
                return;

            element = value;

            MutexUtil.DoWork(mutexName, () =>
            {
                IsolatedStorageSettings conf = IsolatedStorageSettings.ApplicationSettings;

                if (conf.Contains(Key))
                    conf[Key] = value;
                else
                    conf.Add(Key, value);
                conf.Save();
            });
        }

        public static void ClearAll()
        {
            MutexUtil.DoWork(mutexName, () => 
            {
                    IsolatedStorageSettings.ApplicationSettings.Clear();
            });
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
