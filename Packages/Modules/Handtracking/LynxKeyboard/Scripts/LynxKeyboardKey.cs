using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lynx
{
    /// <summary>
    /// Manages a LynxKeyboard key with correct press release behavior & subKeyboard trigger on long press
    /// </summary>
    public class LynxKeyboardKey : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        //INSPECTOR
        public LynxKeyboard lynxKeyboard;
        public TextMeshProUGUI keyText;
        public Button keyButton;
        public LynxSubKeyboard lynxSubKeyboard;
        [Space]
        public string keyStringDisplay;
        public string keyStringInput;
        [Space]
        public bool pointerHoveringKey = false;

        //PUBLIC
        [HideInInspector] public SubKeyboardLayout subKeyboardLayout;

        //PRIVATE
        private float longPressThreshold = 1f;
        private bool longPressed = false;
        private Coroutine holdCoroutine;



        private void Awake()
        {
            if (keyButton == null) keyButton = GetComponent<Button>();

            if (keyButton == null)
            {
                Debug.LogError("No Button component on this gameObject, impossible to continue correctly");
                return;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (keyButton.interactable)
            {
                longPressed = false;
                holdCoroutine = StartCoroutine(HoldCheck());
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (keyButton.interactable)
            {
                if (longPressed == false && pointerHoveringKey) KeyPress();

                if (holdCoroutine != null) StopCoroutine(holdCoroutine);
                longPressed = false;
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            pointerHoveringKey = true;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            pointerHoveringKey = false;

            if (holdCoroutine != null) StopCoroutine(holdCoroutine);
            longPressed = false;
        }



        /// <summary>
        /// Triggers input handling in the LynxKeyboard & resets subKeyboards
        /// </summary>
        public void KeyPress()
        {
            if(lynxKeyboard.currentActiveLynxSubKeyboard != null )
            {
                lynxKeyboard.DisableAllSubKeyboards();
            }
            lynxKeyboard.HandleInput(keyStringInput);
        }
        /// <summary>
        /// Long press enables the key's associated subKeyboard
        /// </summary>
        public void KeyLongPress()
        {
            lynxKeyboard.EnableSubKeyboard(lynxSubKeyboard);
        }
        private IEnumerator HoldCheck()
        {
            yield return new WaitForSeconds(longPressThreshold);
            longPressed = true;
            KeyLongPress();
        }
        /// <summary>
        /// Updates the key's input and display text
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="displayText"></param>
        public void UpdateKeyStrings(string inputText, string displayText)
        {
            keyStringInput = inputText;
            keyStringDisplay = displayText;

            keyText.text = displayText;
        }

    }
}