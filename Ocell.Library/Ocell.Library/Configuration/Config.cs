using AncoraMVVM.Base.Interfaces;


namespace Ocell.Library
{
    public static partial class Config
    {
        const string pushEnabledKey = "PUSHENABLED";

#if OCELL_FULL
        static ConfigItem<bool?> PushEnabledConfigItem = new ConfigItem<bool?>
        {
            Key = pushEnabledKey,
            DefaultValue = null
        };
#endif

        public static bool? PushEnabled
        {
            get
            {
#if OCELL_FULL
                return PushEnabledConfigItem.Value;
#else
                return false;
#endif
            }
            set
            {
#if OCELL_FULL
                PushEnabledConfigItem.Value = value;
#endif
            }
        }
    }
}
