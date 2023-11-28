using UnityEngine;
using UnityEngine.UI;


namespace Lynx.UI
{
    public class LynxImage : Image
    {
        [SerializeField] private bool useTheme = false;
        [SerializeField] private LynxThemeColorSetSO.ThemeInfo selectedColor;

        protected override void Awake()
        {
            if (useTheme && LynxThemeManager.Instance)
            {
                LynxThemeManager.Instance.ThemeUpdateEvent += this.SetThemeColors;
                SetThemeColors();
            }
        }
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public void SetThemeColors()
        {
            if (useTheme)
                color = LynxThemeManager.Instance.currentTheme.GetColor(selectedColor);
        }

        /// <summary>
        /// Define if this element should use the theme manager
        /// </summary>
        /// <param name="enable">True to use theme manager</param>
        public void SetUseTheme(bool enable = true)
        {
            useTheme = enable;
        }

        /// <summary>
        /// Check if current element is using theme manager.
        /// </summary>
        /// <returns>True if this element use theme manager.</returns>
        public bool IsUsingTheme()
        {
            return useTheme;
        }
    }
}

