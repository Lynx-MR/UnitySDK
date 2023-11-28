using UnityEngine;

namespace Lynx.UI
{
    public abstract class LynxThemed : MonoBehaviour
    {
        [Header("LynxThemed")]
        public bool useLynxTheme = true;



        protected virtual void Awake()
        {
            if (LynxThemeManager.Instance == null) LynxThemeManager.CheckLynxThemeManagerInstance();
            if (LynxThemeManager.Instance != null) // [TODO] Currently Instance can still be null when loading the scene in editor
            {
                LynxThemeManager.Instance.ThemeUpdateEvent += UpdateTheme;
                UpdateLynxThemedColorsList();
                UpdateTheme();
            }
        }

        protected virtual void OnDestroy()
        {
            if (LynxThemeManager.Instance != null)
                LynxThemeManager.Instance.ThemeUpdateEvent -= UpdateTheme;
        }


        public abstract void UpdateTheme();

        public virtual void UpdateLynxThemedColorsList()
        {

        }

        public virtual void CheckLynxThemeManagerInstance()
        {
            FindObjectOfType<LynxThemeManager>()?.SetupSingleton();
        }

    }
}