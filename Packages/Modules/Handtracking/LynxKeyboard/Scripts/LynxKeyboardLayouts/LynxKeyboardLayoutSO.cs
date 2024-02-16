using System;
using UnityEngine;

namespace Lynx
{
    /// <summary>
    /// ScriptableObject containing a Lynx KeyboardLayout datastructure
    /// </summary>
    [CreateAssetMenu(fileName = "LynxKeyboardLayout", menuName = "ScriptableObjects/LynxKeyboardLayout", order = 1)]
    [Serializable]
    public class LynxKeyboardLayoutSO : ScriptableObject
    {
        public KeyboardLayout keyboardLayout;
    }
}