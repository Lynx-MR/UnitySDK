//   ==============================================================================
//   | Lynx Interfaces (2023)                                                     |
//   |======================================                                      |
//   | LynxToggleButton Script                                                    |
//   | Script to set a UI element as Toggle Button.                               |
//   ==============================================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lynx.UI
{
    public class LynxToggleButton : Button, IPointerUpHandler, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IInitializePotentialDragHandler
    {
        #region INSPECTOR VARIABLES

        // Button Parameters
        [SerializeField] public UnityEvent OnPress;
        [SerializeField] public UnityEvent OnUnpress;
        [SerializeField] public UnityEvent OnToggle;
        [SerializeField] public UnityEvent OnUntoggle;

        [SerializeField] public bool m_disableSelectState = true;
        [SerializeField] protected bool m_useTheme = false;
        [SerializeField] public bool m_useSound = false;

        [SerializeField] public Graphic[] m_secondaryTargetGraphic;

        [SerializeField] public ButtonAnimation m_animation = new ButtonAnimation();

        #endregion

        #region PRIVATE VARIABLES

        private bool m_isRunning = false; // Avoid multiple press or unpress making the object in unstable state.
        private bool m_isCurrentlyPressed = false; // Status of the current object.
        private bool m_isToggled = false; // Status of the button.
        private bool m_isInteractable = true; // Starting interactable status.

        private ScrollRect scrollRect = null;

        #endregion

        #region UNITY API

        // Awake is called when an enabled script instance is being loaded.
        protected override void Awake()
        {
            // Interactable Patch - Save the initial value of interactable. 
#if !UNITY_EDITOR 
            m_isInteractable = interactable;
#endif

            base.Awake();

            if (m_useTheme)
            {
                LynxThemeMethods.SubscribeThemeUpdate(this);
            }
        }

        // OnEnable is called when the object becomes enabled and active.
        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_useTheme)
            {
                LynxThemeMethods.SubscribeThemeUpdate(this);
            }

            if (m_isToggled)
            {
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                base.OnPointerDown(eventData);
                base.OnDeselect(eventData);
            }

            // Interactable Patch - Wait 0.25 seconds to enable interactability.
#if !UNITY_EDITOR
            StartCoroutine(WaitCoroutine(0.25f, ResetInteractable));
#else
            ResetInteractable(true);
#endif
        }

        // OnDisable is called when the behaviour becomes disabled.
        protected override void OnDisable()
        {
            // Interactable Patch - Disable interactability and reset the press animation.
#if !UNITY_EDITOR
            interactable = false;
            ButtonAnimationMethods.ResetAnimation(m_animation, this.transform);
#endif

            base.OnDisable();

            if (m_useTheme)
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
            if (!IsInteractable()) return;

            base.OnPointerDown(eventData);

            if (m_useSound)
            {
                LynxSoundsMethods.OnPressSound(gameObject.transform.position);
            }

            if (!m_isRunning && !m_isCurrentlyPressed)
            {
                m_isRunning = true;
                StartCoroutine(ButtonAnimationMethods.PressingAnimationCoroutine(m_animation, this.transform, CallbackStopRunning));
                m_isCurrentlyPressed = true;
            }
        }

        // OnPointerUp is called when the mouse click on this selectable UI object is released.
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            if (m_useSound)
            {
                LynxSoundsMethods.OnUnpressSound(gameObject.transform.position);
            }

            if (m_isCurrentlyPressed)
            {
                m_isRunning = true;
                StartCoroutine(ButtonAnimationMethods.UnpressingAnimationCoroutine(m_animation, this.transform, CallbackStopRunning));
                m_isCurrentlyPressed = false;
            }

            if (m_isToggled)
            {
                base.OnPointerUp(eventData);
                m_isToggled = false;
                OnUntoggle.Invoke();
            }
            else
            {
                base.OnPointerDown(eventData);
                m_isToggled = true;
                OnToggle.Invoke();
            }
        }

        // DoStateTranistion is called on every state change to manage graphic modification
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (transition != Transition.ColorTint) return;
            if (m_secondaryTargetGraphic == null) return;

            for (int i = 0; i < m_secondaryTargetGraphic.Length; ++i)
            {
                Color tintColor;
                switch (state)
                {
                    case SelectionState.Normal:
                        tintColor = colors.normalColor;
                        break;
                    case SelectionState.Highlighted:
                        tintColor = colors.highlightedColor;
                        break;
                    case SelectionState.Pressed:
                        tintColor = colors.pressedColor;
                        break;
                    case SelectionState.Selected:
                        tintColor = colors.selectedColor;
                        break;
                    case SelectionState.Disabled:
                        tintColor = colors.disabledColor;
                        break;
                    default:
                        tintColor = Color.black;
                        break;
                }
                m_secondaryTargetGraphic[i].CrossFadeColor(tintColor, instant ? 0f : colors.fadeDuration, true, true);
            }
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (scrollRect == null)
                scrollRect = this.gameObject.GetComponentInParent<ScrollRect>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if(scrollRect != null)
                scrollRect.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (scrollRect != null)
                scrollRect.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (scrollRect != null)
                scrollRect.OnEndDrag(eventData);
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

        /// <summary>
        /// CallbackStopRunning is called when a button animation coroutine is complete.
        /// </summary>
        /// <param name="state">True to call OnPress, false to call OnUnpress.</param>
        private void CallbackStopRunning(bool state)
        {
            m_isRunning = false;

            if (state)
            {
                OnPress.Invoke();
            }
            else
            {
                OnUnpress.Invoke();
            }
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Get the state of the toggle
        /// </summary>
        public bool IsToggled()
        {
            return m_isToggled;
        }

        /// <summary>
        /// Set state for toggle.
        /// </summary>
        /// <param name="toggle">True, to check the toggle. False, to uncheck the toggle.</param>
        public void SetToggle(bool toggle)
        {
            if(toggle != m_isToggled)
            {
                m_isToggled = toggle;
                OnToggle.Invoke();
            }
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
            m_useTheme = enable;
        }

        /// <summary>
        /// Check if current element is using theme manager.
        /// </summary>
        /// <returns>True if this element use theme manager.</returns>
        public bool IsUsingTheme()
        {
            return m_useTheme;
        }

        #endregion

    }
}
