using System.Collections.Generic;
using UnityEngine;

namespace Lynx
{
    [System.Serializable] public class KeyboardLayout
    {
        public string name;
        public List<KeyboardLayer> keyboardLayers = new List<KeyboardLayer>();
    }
    [System.Serializable] public class KeyboardLayer
    {
        public string name;
        public List<KeyboardRow> keyboardRows = new List<KeyboardRow>();
    }

    [System.Serializable] public class SubKeyboardLayout
    {
        public SubKeyboardPositioning positioning;
        public List<SubKeyboardRow> keyboardRows = new List<SubKeyboardRow>();
    }
    public enum SubKeyboardPositioning
    {
        TopFromBottomLeftCorner,
        TopFromBottomRightCorner
    }


    [System.Serializable] public abstract class Row
    {
        public string name;
        public float relHeight = 1;
    }
    [System.Serializable] public class KeyboardRow : Row
    {
        public List<KeyboardKey> keyboardRowKeys = new List<KeyboardKey>();
    }
    [System.Serializable] public class SubKeyboardRow : Row
    {
        public List<SubKeyboardKey> keyboardRowKeys = new List<SubKeyboardKey>();
    }


    [System.Serializable] public abstract class Key
    {
        public string inputString;
        public string displayString;
        public float relWidth = 1;
    }
    [System.Serializable] public class KeyboardKey : Key
    {
        public SubKeyboardLayout subKeyboard;
    }
    [System.Serializable] public class SubKeyboardKey : Key
    {
        
    }


    [System.Serializable] public class LynxKeyboardLayer
    {
        public RectTransform keyboardLayerTransform;
        public List<RectTransform> keyboardRowTransforms = new List<RectTransform>();
        public List<LynxKeyboardKey> lynxKeyboardKeys = new List<LynxKeyboardKey>();
        public List<LynxSubKeyboard> lynxSubKeyboards = new List<LynxSubKeyboard>();
    }
}
