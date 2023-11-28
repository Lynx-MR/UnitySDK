using UnityEngine.Events;

namespace Lynx
{
    [System.Serializable]
    public class CustomUnityStringEvent : UnityEvent<string>
    {
    }

    [System.Serializable]
    public class CustomUnityIntEvent : UnityEvent<int>
    {
    }

    [System.Serializable]
    public class CustomUnityBoolEvent : UnityEvent<bool>
    {
    }
}