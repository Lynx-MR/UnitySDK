//   ==============================================================================
//   | Lynx Interfaces (2023)                                                     |
//   |======================================                                      |
//   | LynxSimpleButton Script                                                    |
//   | Script to set a UI element as Simple Button.                               |
//   ==============================================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lynx.UI
{
    public class LynxSimpleButton : Button
    {
        #region INSPECTOR VARIABLES
        
        // Button Parameters
        [SerializeField] private UnityEvent OnPress;
        [SerializeField] private UnityEvent OnUnpress;

        [SerializeField] private bool m_disableSelectState = true;
        [SerializeField] private bool m_useTheme = false;
        [SerializeField] private bool m_useSound = false;

        [SerializeField] private Graphic[] m_secondaryTargetGraphic;

        [SerializeField] private ButtonAnimation m_animation = new ButtonAnimation();

        #endregion
        
        #region PRIVATE VARIABLES

        private bool m_isRunning = false; // Avoid multiple press or unpress making the object in unstable state.
        private bool m_isCurrentlyPressed = false; // Status of the current object.
        private bool m_isInteractable = true; // Starting interactable status.

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
            m_isCurrentlyPressed = false;
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
                LynxSoundsMethods.OnPressSound();
            }

            if (!m_isRunning && !m_isCurrentlyPressed)
            {
                m_isRunning = true;
                m_isCurrentlyPressed = true;
                StartCoroutine(ButtonAnimationMethods.PressingAnimationCoroutine(m_animation, this.transform, CallbackStopRunning));
            }
        }

        // OnPointerUp is called when the mouse click on this selectable UI object is released.
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            base.OnPointerUp(eventData);

            if (m_useSound)
            {
                LynxSoundsMethods.OnUnpressSound();
            }

            if (m_isCurrentlyPressed)
            {
                m_isRunning = true;
                m_isCurrentlyPressed = false;
                StartCoroutine(ButtonAnimationMethods.UnpressingAnimationCoroutine(m_animation, this.transform, CallbackStopRunning));
            }
        }

        // DoStateTranistion is called on every state change to manage graphic modification
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (transition != Transition.ColorTint) return;

            for(int i = 0; i< m_secondaryTargetGraphic.Length; ++i)
            {
                if(m_secondaryTargetGraphic[i] != null)
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
                else
                {
                    Debug.LogWarning($"Secondary target graphic of {this.gameObject.name} Element {i} is null.", this.gameObject);
                }
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

                /// <summary>
                /// CallbackStopRunning is called when a button animation coroutine is complete.
                /// </summary>
                /// <param name="state">True to call OnUnpress, false to call OnPress.</param>
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

        #region THEME MANAGING

                /// <summary>
                /// change the colorblock of a button to match the selected theme
                /// </summary>
                public void SetThemeColors()
                {
                    if(LynxThemeManager.Instance != null)
                    {
                        colors = LynxThemeManager.Instance.currentTheme.selectableColors;
                    }
                    else
                    {
                        //Debug.LogWarning("There is no Lynx Theme Manager in the scene.", this.gameObject);
                    }
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