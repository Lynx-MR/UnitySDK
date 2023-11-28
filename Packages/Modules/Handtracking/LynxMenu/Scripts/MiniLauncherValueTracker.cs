namespace Lynx
{
    /* Syncs values across all UI & provides a central place manage them from */
    public class MiniLauncherValueTracker : MonoSingleton<MiniLauncherValueTracker>
    {
        public SyncValue<bool> cast = new SyncValue<bool>(false);
        public SyncValue<bool> wifi = new SyncValue<bool>(false);
        public SyncValue<bool> bluetooth = new SyncValue<bool>(false);

        public SyncValue<int> battery = new SyncValue<int>(100);
        public SyncValue<int> volume = new SyncValue<int>(100);

        public SyncValue<bool> indexfinger_keyboard = new SyncValue<bool>(true);
        public SyncValue<bool> tutorial_turnoverhand = new SyncValue<bool>(true);
        public SyncValue<bool> darkmode = new SyncValue<bool>(false);
        public SyncValue<bool> armode = new SyncValue<bool>(false);
        public SyncValue<bool> colourblind = new SyncValue<bool>(false);

        /* Get synced value by string (useful for Unity prefabs!) */
        public SyncValue<bool> GetSyncBoolByName(string name)
        {
            switch (name)
            {
                case "cast":
                    return cast;
                case "wifi":
                    return wifi;
                case "bluetooth":
                    return bluetooth;
                case "darkmode":
                    return darkmode;
                case "armode":
                    return armode;
                case "indexfinger_keyboard":
                    return indexfinger_keyboard;
                case "tutorial_turnoverhand":
                    return tutorial_turnoverhand;
                case "colourblind":
                    return colourblind;
            }
            return null;
        }
        public SyncValue<int> GetSyncIntByName(string name)
        {
            switch (name)
            {
                case "battery":
                    return battery;
                case "volume":
                    return volume;
            }
            return null;
        }
    }

    /* Templated sync value with on-change event */
    public class SyncValue<T>
    {
        public delegate void ValueUpdated(T val);
        public ValueUpdated ValueChanged;

        private T value;

        public SyncValue(T val)
        {
            value = val;
        }

        public void Set(T val)
        {
            value = val;
            ValueChanged?.Invoke(value);
        }
        public T Get()
        {
            return value;
        }
    }
}