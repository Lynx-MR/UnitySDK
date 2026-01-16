//   ==============================================================================
//   | Lynx Interfaces (2023)                                                     |
//   |======================================                                      |
//   | LynxTimerButton Script                                                     |
//   | Hold-to-activate button with visual timer feedback (radial fill)           |
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
    /// Button that requires holding for a specified duration before activation.
    /// Displays visual timer feedback via a radial fill image. Prevents accidental clicks
    /// by requiring sustained press. Useful for destructive actions or important confirmations.
    /// </summary>
    public class LynxTimerButton : Button, IPointerUpHandler, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IInitializePotentialDragHandler
    {
        #region INSPECTOR VARIABLES

        // === Events ===
        // Called when button is pressed down (after animation if enabled)
        [SerializeField] public UnityEvent OnPress;
        // Called when button is released (after animation if enabled)
        [SerializeField] public UnityEvent OnUnpress;
        // Called when timer completes (user held button for full duration)
        [SerializeField] public UnityEvent OnTimerPress;

        // === Behavior Settings ===
        [SerializeField] public bool m_disableSelectState = true;  // Prevents "selected" state which can interfere with expected button behavior
        [SerializeField] protected bool m_useTheme = false;        // Automatically apply colors from LynxThemeManager
        [SerializeField] public bool m_useSound = false;           // Play press/unpress sounds via LynxSoundsMethods
        [SerializeField] public bool m_disableOnDrag = true;       // If true, cancels timer when dragged beyond threshold (useful for scroll views)
        [SerializeField] public bool m_useAnimation = true;        // Enable press/unpress animations

        // === Visual Settings ===
        // Additional graphics to apply color transitions to (beyond the main targetGraphic)
        [SerializeField] public Graphic[] m_secondaryTargetGraphic;

        // === Animation Settings ===
        [SerializeField] public ButtonAnimation m_animation = new ButtonAnimation();  // Animation configuration (scale, duration, etc.)

        // === Timer-Specific Settings ===
        [SerializeField] public Image m_timerImage = null;  // Radial fill image that visualizes timer progress (0 to 1)
        [SerializeField] public float m_deltaTime = 2.0f;   // Duration in seconds user must hold button before OnTimerPress fires

        #endregion

        #region PRIVATE VARIABLES

        // === State Management ===
        private bool m_isRunning = false;           // Prevents overlapping animations (locks during animation coroutine)
        private bool m_isCurrentlyPressed = false;  // Tracks whether button is currently in pressed state
        private bool m_timerIsRunning = false;      // Tracks whether timer countdown is currently active
        private bool m_isInteractable = true;       // Stores initial interactable value for restoration after enable delay

        // === Timer Management ===
        private IEnumerator timerCoroutine = null;  // Reference to running timer coroutine (for cancellation)

        // === Drag Handling ===
        private ScrollRect scrollRect = null;       // Cached reference to parent ScrollRect (if any)
        private Vector3 m_dragStartPos;             // Starting position for drag distance calculation

        #endregion

        #region UNITY API

        /// <summary>
        /// Initialize button state and subscribe to theme updates if enabled.
        /// </summary>
        protected override void Awake()
        {
            // WORKAROUND: On device (non-editor), save initial interactable state
            // to restore it after the OnEnable delay (see OnEnable for details)
#if !UNITY_EDITOR
            m_isInteractable = interactable;
#endif

            base.Awake();

            if (m_useTheme)
            {
                LynxThemeMethods.SubscribeThemeUpdate(this);
            }
        }

        /// <summary>
        /// Re-subscribe to theme and restore interactable state with delay.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_useTheme)
            {
                LynxThemeMethods.SubscribeThemeUpdate(this);
            }

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
        /// Handle pointer/touch down: play sound, start press animation, and begin timer countdown.
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

            // Start the timer countdown if not already running
            if (!m_timerIsRunning)
            {
                m_timerIsRunning = true;
                timerCoroutine = TimerAnimationCoroutine();
                StartCoroutine(timerCoroutine);  // Begin filling the timer image
            }
        }

        /// <summary>
        /// Handle pointer/touch release: play sound, start unpress animation, and cancel timer.
        /// If released before timer completes, OnTimerPress will NOT fire.
        /// </summary>
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            base.OnPointerUp(eventData);

            // Play unpress sound at button's world position
            if (m_useSound)
            {
                LynxSoundsMethods.OnUnpressSound(gameObject.transform.position);
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
                else
                    OnUnpress.Invoke();  // No animation: fire event immediately
            }

            // Cancel timer and reset visual feedback
            m_timerIsRunning = false;
            StopCoroutine(timerCoroutine);
            m_timerImage.fillAmount = 0.0f;  // Reset fill to empty
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
            if (scrollRect == null)
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
        /// Handle ongoing drag: forward to ScrollRect and cancel timer if dragged too far.
        /// This prevents timer completion when user intended to scroll.
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
            
            // If dragged beyond threshold (0.04 units), cancel the timer
            // This prevents accidental timer completion when user intended to scroll
            if (dist > 0.04f && m_disableOnDrag)
            {
                m_timerIsRunning = false;
                StopCoroutine(timerCoroutine);
                m_timerImage.fillAmount = 0.0f;  // Reset fill to empty
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

        #region ANIMATION COROUTINES

        /// <summary>
        /// Animate the timer fill image from 0 to 1 over m_deltaTime seconds.
        /// If timer completes without interruption, fires OnTimerPress event.
        /// </summary>
        private IEnumerator TimerAnimationCoroutine()
        {
            float elapsedTime = 0.0f;

            // Gradually fill the timer image while button is held
            while (elapsedTime < m_deltaTime)
            {
                elapsedTime += Time.deltaTime;
                if(m_useAnimation)
                    m_timerImage.fillAmount = elapsedTime / m_deltaTime;  // Progress from 0 to 1
                yield return new WaitForEndOfFrame();
            }

            // Timer completed! Reset visual and fire completion event
            m_timerImage.fillAmount = 0.0f;

            OnTimerPress.Invoke();  // User successfully held button for full duration

            m_timerIsRunning = false;
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
