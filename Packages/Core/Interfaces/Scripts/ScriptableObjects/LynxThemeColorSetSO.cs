using System;
using UnityEngine;
using UnityEngine.UI;

namespace Lynx.UI
{
    [CreateAssetMenu(fileName = "LynxThemeColorSet", menuName = "ScriptableObjects/LynxThemeColorSet", order = 1)]
    [Serializable]
    public class LynxThemeColorSetSO : ScriptableObject
    {
        public string themeName;
        public enum ThemeInfo
        {
            Normal,
            Highlithed,
            Pressed,
            Selected,
            Disabled,
            Accent,
            BackgroundMain,
            BackgroundDark,
            BackgroundLight,
            Icon,
            Text
        }

        [SerializeField] private Color normal;
        [SerializeField] private Color highlithed;
        [SerializeField] private Color pressed;
        [SerializeField] private Color selected;
        [SerializeField] private Color disabled;
        [SerializeField] private float colorMultiplier = 1.0f;
        [SerializeField] private float fadeDuration = 0.33f;
        [Space]
        [SerializeField] private Color accent;


        [SerializeField] private Color main;
        [SerializeField] private Color dark;
        [SerializeField] private Color light;


        [SerializeField] private Color text;
        [SerializeField] private Color icon;

        /// <summary>
        /// ColorBlock used by selectable registered in themeManager
        /// </summary>
        public ColorBlock selectableColors
        {
            get
            {
                return CreateColorBlock();
            }
        }

        private void OnValidate()
        {
            LynxThemeManager.CheckLynxThemeManagerInstance();
            if (LynxThemeManager.Instance) LynxThemeManager.Instance.UpdateLynxThemedComponents();
        }

        private ColorBlock CreateColorBlock()
        {
            ColorBlock newBlock = new ColorBlock();
            newBlock.normalColor = normal;
            newBlock.highlightedColor = highlithed;
            newBlock.pressedColor = pressed;
            newBlock.selectedColor = selected;
            newBlock.disabledColor = disabled;
            newBlock.colorMultiplier = colorMultiplier;
            newBlock.fadeDuration = fadeDuration;
            return newBlock;
        }

        /// <summary>
        /// Return the color coresponding to the input
        /// </summary>
        /// <param name="colorType">color type set in the theme</param>
        /// <returns></returns>
        public Color GetColor(ThemeInfo colorType)
        {
            switch (colorType)
            {
                case ThemeInfo.Normal:
                    return normal;
                case ThemeInfo.Highlithed:
                    return highlithed;
                case ThemeInfo.Pressed:
                    return pressed;
                case ThemeInfo.Selected:
                    return selected;
                case ThemeInfo.Disabled:
                    return disabled;
                case ThemeInfo.Accent:
                    return accent;
                case ThemeInfo.BackgroundMain:
                    return main;
                case ThemeInfo.BackgroundDark:
                    return dark;
                case ThemeInfo.BackgroundLight:
                    return light;
                case ThemeInfo.Icon:
                    return icon;
                case ThemeInfo.Text:
                    return text;
            }
            return Color.white;
        }

        /// <summary>
        /// return the fade duration set in the theme
        /// </summary>
        /// <returns></returns>
        public float GetFadeDuration()
        {
            return fadeDuration;
        }
    }
}