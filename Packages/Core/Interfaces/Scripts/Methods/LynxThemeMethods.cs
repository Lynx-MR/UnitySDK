using UnityEngine.UI;

namespace Lynx.UI
{
    public class LynxThemeMethods
    {
        /// <summary>
        /// Call this function to subscribe a selectable object to the Theme Update Event.
        /// </summary>
        /// <param name="selectable"></param>
        public static void SubscribeThemeUpdate(Selectable selectable)
        {
            if (LynxThemeManager.Instance != null)
            {
                LynxThemeManager.Instance.ThemeUpdateEvent += () => SetThemeColors(selectable);
                SetThemeColors(selectable);
            }
            else
            {
                //Debug.LogWarning("There is no Lynx Theme Manager in the scene.", button.gameObject);
            }
        }

        /// <summary>
        /// Call this function to unsubscribe a selectable object to the Theme Update Event.
        /// </summary>
        /// <param name="selectable"></param>
        public static void UnsubscribeThemeUpdate(Selectable selectable)
        {
            if (LynxThemeManager.Instance != null)
            {
                LynxThemeManager.Instance.ThemeUpdateEvent -= () => SetThemeColors(selectable);
                SetThemeColors(selectable);
            }
            else
            {
                //Debug.LogWarning("There is no Lynx Theme Manager in the scene.", button.gameObject);
            }
        }

        /// <summary>
        /// Call this function to change the ColorBlock of a selectable object to match the selected theme.
        /// </summary>
        public static void SetThemeColors(Selectable selectable)
        {
            if (LynxThemeManager.Instance != null)
            {
                selectable.colors = LynxThemeManager.Instance.currentTheme.selectableColors;
            }
            else
            {
                //Debug.LogWarning("There is no Lynx Theme Manager in the scene.",button.gameObject);
            }
        }
    }
}
