using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Lynx.UI
{

    public class LynxScrollbar : Scrollbar
    {
        [SerializeField] private bool m_disableSelectState = true;
        [SerializeField] private bool useTheme = false;

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

            PointerEventData eventData = new PointerEventData(EventSystem.current);
            //base.OnPointerDown(eventData);
            base.OnDeselect(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            // State Select can affect the expected behaviour of the button.
            // It is natively deactivated on our buttons.
            // But can be reactivated by unchecking disableSelectState

            if (m_disableSelectState)
            {
                base.OnDeselect(eventData);
            }
            else
            {
                base.OnSelect(eventData);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (LynxThemeManager.Instance.currentSounds.CallAudioOnPress(out AudioClip clip) && useTheme)
            {
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (LynxThemeManager.Instance.currentSounds.CallOnAudioUnpress(out AudioClip clip) && useTheme)
            {
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            }
        }
        #region THEME MANAGING

        /// <summary>
        /// change the colorblock of a button to match the selected theme
        /// </summary>
        public void SetThemeColors()
        {
            if (LynxThemeManager.Instance == null)
                return;
            colors = LynxThemeManager.Instance.currentTheme.selectableColors;
        }
        #endregion

    }
}
