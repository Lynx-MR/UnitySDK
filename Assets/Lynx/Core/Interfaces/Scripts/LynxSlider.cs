//   ==============================================================================
//   | Lynx Interfaces (2023)                                                     |
//   |======================================                                      |
//   | LynxSlider Script                                                          |
//   | Script to set a UI element as Slider.                                      |
//   ==============================================================================


using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Lynx.UI
{
    public class LynxSlider : Slider
    {
        #region INSPECTOR VARIABLES

        [SerializeField] private bool m_disableSelectState = true;
        [SerializeField] private bool useTheme = false;
        [SerializeField] private bool useSound = false;

        #endregion

        #region PRIVATE VARIABLES

        private bool m_isInteractable = true;

        #endregion

        #region UNITY API

        // Awake is called when an enabled script instance is being loaded.
        protected override void Awake()
        {
            // Interactable Patch - Save the initial value of interactable. 
            m_isInteractable = interactable;

            base.Awake();

            if (useTheme)
            {
                LynxThemeMethods.SubscribeThemeUpdate(this);
            }
        }

        // OnEnable is called when the object becomes enabled and active.
        protected override void OnEnable()
        {
            base.OnEnable();

            if (useTheme)
            {
                LynxThemeMethods.SubscribeThemeUpdate(this);
            }

            // Interactable Patch - Wait 0.25 seconds to enable interactability.
            StartCoroutine(WaitCoroutine(0.25f, ResetInteractable));
        }

        // OnDisable is called when the behaviour becomes disabled.
        protected override void OnDisable()
        {
            // Interactable Patch - Disable interactability and reset the press animation.
            interactable = false;

            base.OnDisable();

            if (useTheme)
            {
                LynxThemeMethods.UnsubscribeThemeUpdate(this);
            }
        }

        // OnSelect is called when the selectable UI object is selected.
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

        // OnPointerDown is called when the mouse is clicked over this selectable UI object.
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (useSound)
            {
                LynxSoundsMethods.OnPressSound();
            }
        }

        // OnPointerUp is called when the mouse click on this selectable UI object is released.
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            if (useSound)
            {
                LynxSoundsMethods.OnUnpressSound();
            }
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Call this coroutine to waiting time.
        /// </summary>
        /// <param name="waitingTime">Time to wait.</param>
        /// <param name="callback">Function to call at the end.</param>
        /// <returns></returns>
        public static IEnumerator WaitCoroutine(float waitingTime, Action<bool> callback)
        {
            yield return new WaitForSeconds(waitingTime);
            callback(false);
        }

        /// <summary>
        /// Call this function to update interactable state of the button.
        /// </summary>
        /// <param name="boolean"></param>
        private void ResetInteractable(bool boolean)
        {
            interactable = m_isInteractable;
        }

        #endregion

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
        #endregion

    }

}
