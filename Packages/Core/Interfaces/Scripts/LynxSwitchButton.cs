//   ==============================================================================
//   | Lynx Interfaces (2023)                                                     |
//   |======================================                                      |
//   | LynxSwitchButton Script                                                    |
//   | Toggle/Switch Button with animated handle and persistent on/off state     |
//   ==============================================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lynx.UI
{
    /// <summary>
    /// Toggle button component with sliding handle animation and persistent state.
    /// Extends Button with on/off toggle functionality, animations, sounds, and theme support.
    /// The handle visually slides between positions to indicate the current toggle state.
    /// </summary>
    public class LynxSwitchButton : Button, IPointerUpHandler, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IInitializePotentialDragHandler
    {
        #region INSPECTOR VARIABLES

        // === Events ===
        // Called when button is pressed down (after animation if enabled)
        [SerializeField] public UnityEvent OnPress;
        // Called when button is released (after animation if enabled)
        [SerializeField] public UnityEvent OnUnpress;
        // Called when switch is toggled ON
        [SerializeField] public UnityEvent OnToggle;
        // Called when switch is toggled OFF
        [SerializeField] public UnityEvent OnUntoggle;

        // === Behavior Settings ===
        [SerializeField] public bool m_disableSelectState = true;  // Prevents "selected" state which can interfere with expected button behavior
        [SerializeField] public bool m_disableOnDrag = false;      // If true, cancels press when dragged beyond threshold (useful for scroll views)
        [SerializeField] protected bool m_useTheme = false;        // Automatically apply colors from LynxThemeManager
        [SerializeField] public bool m_useSound = false;           // Play press/unpress sounds via LynxSoundsMethods

        // === Visual Settings ===
        // Additional graphics to apply color transitions to (beyond the main targetGraphic)
        [SerializeField] public Graphic[] m_secondaryTargetGraphic;

        // === Animation Settings ===
        [SerializeField] public bool m_useAnimation = true;                    // Enable press/unpress animations
        [SerializeField] public ButtonAnimation m_animation = new ButtonAnimation();  // Animation configuration (scale, duration, etc.)

        // === Switch-Specific Settings ===
        [SerializeField] private Transform m_handle = null;  // The handle transform that slides between on/off positions
        [SerializeField] public float m_lerpTime = 0.33f;    // Duration of the handle slide animation in seconds

        #endregion

        #region PRIVATE VARIABLES

        // === State Management ===
        private bool m_isRunning = false;           // Prevents overlapping animations (locks during animation coroutine)
        private bool m_isCurrentlyPressed = false;  // Tracks whether button is currently in pressed state
        private bool m_isToggled = false;           // Current toggle state: true = ON, false = OFF
        private bool m_isInteractable = true;       // Stores initial interactable value for restoration after enable delay
        private bool m_enableStateCheck = false;    // Flag to check if visual display matches toggle state on enable

        // === Handle Position Tracking ===
        private Vector3 offHandlePosition;          // Handle's local position when switch is OFF (left side)
        private Vector3 onHandlePosition;           // Handle's local position when switch is ON (right side)

        // === Drag Handling ===
        private Vector3 m_dragStartPos;             // Starting position for drag distance calculation
        private ScrollRect scrollRect = null;       // Cached reference to parent ScrollRect (if any)

        #endregion

        #region UNITY API

        /// <summary>
        /// Initialize handle positions and subscribe to theme updates if enabled.
        /// Calculates the on/off positions for the handle based on its initial offset.
        /// </summary>
        protected override void Awake()
        {
            // WORKAROUND: On device (non-editor), save initial interactable state
            // to restore it after the OnEnable delay (see OnEnable for details)
#if !UNITY_EDITOR
            m_isInteractable = interactable;
#endif

            // Calculate handle positions: use current X offset to determine left/right positions
            float handleOffset = Mathf.Abs(m_handle.localPosition.x);
            onHandlePosition = new Vector3(handleOffset, 0, 0);   // Right side (ON)
            offHandlePosition = new Vector3(-handleOffset, 0, 0); // Left side (OFF)

            base.Awake();

            if (m_useTheme)
            {
                LynxThemeMethods.SubscribeThemeUpdate(this);
            }
        }

        /// <summary>
        /// Re-subscribe to theme, sync visual state with toggle state, and restore interactable state with delay.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_useTheme)
            {
                LynxThemeMethods.SubscribeThemeUpdate(this);
            }

            // If switch is toggled ON, ensure visual state reflects this
            if (m_isToggled)
            {
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                base.OnPointerDown(eventData);  // Set to pressed visual state
                base.OnDeselect(eventData);     // But don't keep it selected
            }

            // Force handle position to match current toggle state
            StartCoroutine(ToggleAnimationCoroutine());

            // WORKAROUND: On device, delay enabling interactability by 0.25s to prevent
            // accidental immediate activation when button becomes active
#if !UNITY_EDITOR
            StartCoroutine(WaitCoroutine(0.25f, ResetInteractable));
#else
            ResetInteractable(true);
#endif
        }

        /// <summary>
        /// Clean up: unsubscribe from theme, disable interactability, and reset visual state.
        /// </summary>
        protected override void OnDisable()
        {
            // WORKAROUND: On device, ensure button is non-interactable and visually reset
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

        /// <summary>
        /// Override select behavior to prevent unwanted "selected" state by default.
        /// The selected state can interfere with button behavior in VR/XR contexts.
        /// </summary>
        public override void OnSelect(BaseEventData eventData)
        {
            // By default, prevent the button from entering "selected" state
            // This can be re-enabled by setting m_disableSelectState to false in the inspector
            if (m_disableSelectState)
            {
                base.OnDeselect(eventData);  // Force deselect instead
            }
            else
            {
                base.OnSelect(eventData);
            }
        }

        /// <summary>
        /// Handle pointer/touch down: play sound, start press animation, and invoke OnPress event.
        /// This is the first half of the toggle interaction.
        /// </summary>
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            base.OnPointerDown(eventData);

            // Play press sound at button's world position (for spatial audio)
            if (m_useSound)
            {
                LynxSoundsMethods.OnPressSound(gameObject.transform.position);
            }

            // Only trigger press if not already running an animation and not already pressed
            if (!m_isRunning && !m_isCurrentlyPressed)
            {
                m_isCurrentlyPressed = true;
                if (m_useAnimation)
                {
                    m_isRunning = true;  // Lock to prevent overlapping animations
                    StartCoroutine(ButtonAnimationMethods.PressingAnimationCoroutine(m_animation, this.transform, CallbackStopRunning));
                }
                else
                    OnPress.Invoke();  // No animation: fire event immediately
            }
        }

        /// <summary>
        /// Handle pointer/touch release: toggle the switch state, animate handle, and invoke appropriate events.
        /// This is where the actual toggle happens - the state flips on release, not on press.
        /// </summary>
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            // Play unpress sound at button's world position
            if (m_useSound)
            {
                LynxSoundsMethods.OnUnpressSound(gameObject.transform.position);
            }

            // Toggle state logic: flip between ON and OFF
            if (m_isToggled && m_isCurrentlyPressed)
            {
                // Currently ON → switch to OFF
                base.OnPointerUp(eventData);  // Update visual state to normal
                m_isToggled = false;
                StartCoroutine(ToggleAnimationCoroutine());  // Slide handle to OFF position
                OnUntoggle.Invoke();  // Fire untoggle event
            }
            else if(m_isCurrentlyPressed)
            {
                // Currently OFF → switch to ON
                base.OnPointerDown(eventData);  // Keep visual state pressed
                m_isToggled = true;
                StartCoroutine(ToggleAnimationCoroutine());  // Slide handle to ON position
                OnToggle.Invoke();  // Fire toggle event
            }

            // Complete the press/unpress cycle with animation
            if (m_isCurrentlyPressed)
            {
                m_isCurrentlyPressed = false;
                if (m_useAnimation)
                {
                    m_isRunning = true;  // Lock to prevent overlapping animations
                    StartCoroutine(ButtonAnimationMethods.UnpressingAnimationCoroutine(m_animation, this.transform, CallbackStopRunning));
                }
            }
        }


        /// <summary>
        /// Apply color transitions to both primary and secondary graphics based on button state.
        /// This extends Unity's default behavior to support multiple graphics changing color together.
        /// </summary>
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);  // Handle primary targetGraphic

            // Only process secondary graphics if using ColorTint transition mode
            if (transition != Transition.ColorTint) return;
            if (m_secondaryTargetGraphic == null) return;

            // Apply the same color transition to all secondary graphics
            for (int i = 0; i < m_secondaryTargetGraphic.Length; ++i)
            {
                // Determine which color to use based on current state
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
                // Smoothly fade to the new color (or instant if specified)
                m_secondaryTargetGraphic[i].CrossFadeColor(tintColor, instant ? 0f : colors.fadeDuration, true, true);
            }
        }

        /// <summary>
        /// Initialize drag tracking: cache parent ScrollRect and record starting position.
        /// This enables both button clicks and scroll gestures to work together.
        /// </summary>
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            // Find parent ScrollRect (if any) for forwarding drag events
            if(scrollRect == null)
                scrollRect = this.gameObject.GetComponentInParent<ScrollRect>();
            
            // Store drag start position in local space (ignoring Z axis for 2D distance calculation)
            m_dragStartPos = Quaternion.Inverse(eventData.pointerDrag.transform.rotation) * eventData.pointerCurrentRaycast.worldPosition;
            m_dragStartPos.Scale(new Vector3(1, 1, 0));
        }

        /// <summary>
        /// Forward drag start event to parent ScrollRect if present.
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (scrollRect != null)
                scrollRect.OnBeginDrag(eventData);
        }

        /// <summary>
        /// Handle ongoing drag: forward to ScrollRect and optionally cancel button press if dragged too far.
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            // Forward drag events to parent ScrollRect for scrolling
            if (scrollRect != null)
                scrollRect.OnDrag(eventData);
            
            // Calculate how far the pointer has moved from start position
            Vector3 endDragPos = Quaternion.Inverse(eventData.pointerDrag.transform.rotation) * eventData.pointerCurrentRaycast.worldPosition;
            endDragPos.Scale(new Vector3(1, 1, 0));
            float dist = Vector3.Distance(m_dragStartPos, endDragPos);
            
            // If dragged beyond threshold (0.04 units), cancel the button press
            // This prevents accidental button clicks when user intended to scroll
            if (dist > 0.04f && m_disableOnDrag)
            {
                m_isCurrentlyPressed = false;
            }
        }

        /// <summary>
        /// Forward drag end event to parent ScrollRect if present.
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (scrollRect != null)
                scrollRect.OnEndDrag(eventData);
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Utility coroutine for delayed execution. Used for the interactable patch workaround.
        /// </summary>
        /// <param name="waitingTime">Time to wait in seconds.</param>
        /// <param name="callback">Function to call after waiting (parameter is always false).</param>
        public static IEnumerator WaitCoroutine(float waitingTime, Action<bool> callback)
        {
            yield return new WaitForSeconds(waitingTime);
            callback(false);
        }

        /// <summary>
        /// Restore the button's interactable state to its initial value.
        /// Called after the OnEnable delay to prevent accidental activation.
        /// </summary>
        /// <param name="boolean">Unused parameter (kept for callback signature compatibility).</param>
        private void ResetInteractable(bool boolean)
        {
            interactable = m_isInteractable;
        }

        /// <summary>
        /// Animation completion callback: unlocks animation state and fires the appropriate event.
        /// This ensures OnPress/OnUnpress events fire AFTER animations complete.
        /// </summary>
        /// <param name="state">True to invoke OnPress event, false to invoke OnUnpress event.</param>
        private void CallbackStopRunning(bool state)
        {
            m_isRunning = false;  // Unlock animation state

            // Fire the appropriate event based on whether this was a press or unpress animation
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
        /// Get the current toggle state of the switch.
        /// </summary>
        /// <returns>True if switch is ON, false if OFF.</returns>
        public bool IsToggled()
        {
            return m_isToggled;
        }


        /// <summary>
        /// Set the toggle state with visual transition and invoke events if state changes.
        /// Use this when you want both the animation and the corresponding events to fire.
        /// </summary>
        /// <param name="state">True to toggle ON, false to toggle OFF.</param>
        public void SetToggleWithTransition(bool state)
        {
            // Only proceed if state is actually changing
            if (m_isToggled != state)
            {
                m_isToggled = state;
                
                // If object is active, animate and fire events
                if (gameObject.activeInHierarchy)
                {
                    if (state) OnToggle.Invoke();
                    else OnUntoggle.Invoke();
                    StartCoroutine(ToggleAnimationCoroutine());  // Animate handle
                }
                else
                {
                    // If inactive, just snap handle to position without animation
                    m_handle.localPosition = m_isToggled ? onHandlePosition : offHandlePosition;
                }
                
                // Update visual state to match toggle state
                if (state) DoStateTransition(SelectionState.Pressed, false);
                else DoStateTransition(SelectionState.Normal, false);
            }
        }

        /// <summary>
        /// Set the toggle state programmatically with optional event invocation.
        /// Use this for silent state changes or when you want fine control over event firing.
        /// </summary>
        /// <param name="state">True to toggle ON, false to toggle OFF.</param>
        /// <param name="launchEvent">If true, invoke OnToggle/OnUntoggle events when state changes.</param>
        public void SetToggle(bool state, bool launchEvent = false)
        {
            // Optionally fire events if state is changing
            if (launchEvent && m_isToggled != state)
            {
                if (state)
                    OnToggle.Invoke();
                else
                    OnUntoggle.Invoke();
            }
            
            // Animate handle if state changed and component is enabled
            if (m_isToggled != state && this.enabled)
                StartCoroutine(ToggleAnimationCoroutine());
            
            m_isToggled = state;
        }

        #endregion

        #region ANIMATION COROUTINES

        /// <summary>
        /// Smoothly animate the handle sliding between ON and OFF positions.
        /// Uses cubic ease in-out for a natural, polished feel.
        /// </summary>
        private IEnumerator ToggleAnimationCoroutine()
        {
            // Determine target position based on current toggle state
            Vector3 targetPos = m_isToggled ? onHandlePosition : offHandlePosition;
            Vector3 startPos = m_handle.localPosition;
            
            // Lerp from start to target over m_lerpTime seconds with easing
            for(float i = 0; i < 1; i+= Time.deltaTime / m_lerpTime)
            {
                i = Mathf.Min(i, 1);  // Clamp to 1.0 to ensure we reach target
                // Apply cubic ease in-out for smooth acceleration and deceleration
                m_handle.localPosition = Vector3.Lerp(startPos, targetPos, LynxMath.Ease(i,LynxMath.easingType.CubicInOut));
                yield return new WaitForEndOfFrame();
            }
        }

        #endregion

        #region THEME MANAGING

        /// <summary>
        /// Apply colors from the LynxThemeManager to this button.
        /// Called automatically when theme changes if m_useTheme is enabled.
        /// </summary>
        public void SetThemeColors()
        {
            if (LynxThemeManager.Instance == null)
                return;
            colors = LynxThemeManager.Instance.currentTheme.selectableColors;
        }

        /// <summary>
        /// Enable or disable automatic theme integration for this button.
        /// When enabled, button colors will update automatically when the theme changes.
        /// </summary>
        /// <param name="enable">True to use theme manager, false to use manual colors.</param>
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